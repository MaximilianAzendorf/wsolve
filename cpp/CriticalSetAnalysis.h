#pragma once

#include <execution>

#include "Types.h"
#include "Util.h"
#include "CriticalSet.h"
#include "InputData.h"
#include "Status.h"

class CriticalSetAnalysis
{
private:
    vector<CriticalSet> _sets;
    InputData const& _inputData;
    int preferenceBound;

    void analyze()
    {
        auto nextOutput = time_now() + ProgressInterval;

        vector<int> newSet;

        int prefIdx = 0;
        for(auto prefIt = _inputData.preference_levels().rbegin(); prefIt != _inputData.preference_levels().rend(); prefIt++, prefIdx++)
        {
            int pref = *prefIt;
            for(int p = 0; p < _inputData.participant_count(); p++)
            {
                if(time_now() > nextOutput)
                {
                    float progress = (float)prefIdx / (float)_inputData.preference_levels().size()
                            + (1.0f / _inputData.preference_levels().size())
                                * ((float)p / (float)_inputData.participant_count());

                    Status::info("    " + str(100 * progress, 2)
                        + "% (pref. " + str(pref) + "/" + str(_inputData.preference_levels().size())
                        + ", participant " + str(p) + "/" + str(_inputData.participant_count()) + "); "
                        + str(_sets.size()) + " sets so far.");

                    nextOutput = time_now() + ProgressInterval;
                }

                newSet.clear();
                int minCount = 0;

                for(int w = 0; w < _inputData.workshop_count(); w++)
                {
                    if(_inputData.participant(p).preference(w) <= pref)
                    {
                        newSet.push_back(w);
                        minCount += _inputData.workshop(w).min();
                    }
                }

                if(minCount > _inputData.participant_count() * (_inputData.slot_count() - 1))
                {
                    // It is impossible that this critical set is not fulfilled by any solution.
                    continue;
                }

                CriticalSet c(pref, newSet.begin(), newSet.end());

                bool isCovered = std::any_of(std::execution::par_unseq, _sets.begin(), _sets.end(),
                        [c](CriticalSet const& other){ return c.is_covered_by(other); });

                if(!isCovered)
                {
                    _sets.push_back(c);
                }
            }
        }

        list<CriticalSet*> setList;
        for(CriticalSet& set : _sets)
        {
            setList.push_back(&set);
        }

        for(int i = 0; i < _sets.size(); i++)
        {
            CriticalSet const& set = _sets[i];

            if(time_now() > nextOutput)
            {
                float progress = (float)i / (float)_sets.size();

                Status::info("    " + str(100 * progress, 2) + "% Simplifying ("
                    + str(i) + "/" + str(_sets.size()) + "); " + str(setList.size()) + " found.");

                nextOutput = time_now() + ProgressInterval;
            }

            for(auto setPtrIt = setList.begin(); setPtrIt != setList.end(); setPtrIt++)
            {
                CriticalSet* setPtr = *setPtrIt;
                bool canBeRemoved = false;

                for(CriticalSet* otherSetPtr : setList)
                {
                    if(setPtr != otherSetPtr && setPtr->is_covered_by(*otherSetPtr))
                    {
                        canBeRemoved = true;
                        break;
                    }
                }

                if(canBeRemoved)
                {
                    setList.erase(setPtrIt);
                    break;
                }
            }
        }
    }

public:
    inline static const seconds ProgressInterval = seconds(3);

    CriticalSetAnalysis(InputData const& inputData, bool analyze = true)
            : _inputData(inputData)
    {
        if(analyze)
        {
            this->analyze();
        }

        preferenceBound = _inputData.max_preference();
        for(int prefLevel : _inputData.preference_levels())
        {
            if(for_preference(prefLevel).front().size() >= _inputData.slot_count())
            {
                preferenceBound = std::min(preferenceBound, prefLevel);
            }
        }
    }

    [[nodiscard]] vector<CriticalSet> for_preference(int preference) const
    {
        list<CriticalSet> relevantSets;
        for(CriticalSet set : _sets)
        {
            if(set.preference() >= preference)
            {
                relevantSets.push_back(set);
            }
        }

        bool changed = true;
        while(changed)
        {
            changed = false;
            for(auto it = relevantSets.begin(); it != relevantSets.end() && !changed; it++)
            {
                CriticalSet& set = *it;
                for(CriticalSet& other : relevantSets)
                {
                    if(&set == &other) continue;
                    if(other.is_subset_of(set))
                    {
                        changed = true;
                        relevantSets.erase(it);
                        break;
                    }
                }
            }
        }

        return vector<CriticalSet>(relevantSets.begin(), relevantSets.end());
    }

    [[nodiscard]] vector<CriticalSet> const& sets() const { return _sets; }

    [[nodiscard]] int preference_bound() const { return preferenceBound; }

    [[nodiscard]] static CriticalSetAnalysis empty(InputData const& inputData)
    {
        return CriticalSetAnalysis(inputData, false);
    }
};


