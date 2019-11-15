using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WSolve
{
    public class Solution
    {
        public Solution(InputData inputData, IEnumerable<(int workshop, int slot)> scheduling,
            IEnumerable<(int participant, int workshop)> assignment)
        {
            InputData = inputData;
            Scheduling = new ReadOnlyDictionary<int, int>(
                new Dictionary<int, int>(scheduling.Select(x => new KeyValuePair<int, int>(x.workshop, x.slot))));
            Assignment = new ReadOnlyDictionary<int, IReadOnlyList<int>>(
                new Dictionary<int, IReadOnlyList<int>>(assignment.GroupBy(kvp => kvp.participant).Select(x =>
                    new KeyValuePair<int, IReadOnlyList<int>>(
                        x.Key,
                        x.Select(w => w.workshop).ToList().AsReadOnly()))));
        }

        public InputData InputData { get; }

        public IReadOnlyDictionary<int, int> Scheduling { get; }

        public IReadOnlyDictionary<int, IReadOnlyList<int>> Assignment { get; }

        public IEnumerable<(int participant, int workshop)> FlatAssignment =>
            Assignment.SelectMany(kvp => kvp.Value.Select(x => (kvp.Key, x)));

        public void Verify()
        {
            void workshopMinConstraint(int w)
            {
                if (FlatAssignment.Count(kvp => kvp.workshop == w) < InputData.Workshops[w].min)
                {
                    throw new VerifyException($"Workshop '{InputData.Workshops[w].name}' has too few participants.");
                }
            }

            void workshopMaxConstraint(int w)
            {
                if (FlatAssignment.Count(kvp => kvp.workshop == w) > InputData.Workshops[w].max)
                {
                    throw new VerifyException($"Workshop '{InputData.Workshops[w].name}' has too many participants.");
                }
            }

            void conductorsAreInOwnWorkshop(int w)
            {
                foreach (int conductor in InputData.Workshops[w].conductors)
                {
                    if (!Assignment[conductor].Contains(w))
                    {
                        throw new VerifyException(
                            $"Participant '{InputData.Participants[conductor]}', conductor of '{InputData.Workshops[w].name}', is not in his own workshop.");
                    }
                }
            }

            void oneWorkshopPerSlot(int p, int s)
            {
                int c = FlatAssignment.Count(kvp => kvp.participant == p && Scheduling[kvp.workshop] == s);
                if (c < 1)
                {
                    throw new VerifyException(
                        $"Participant '{InputData.Participants[p].name}' has no workshop in slot '{InputData.Slots[s]}'.");
                }

                if (c > 1)
                {
                    throw new VerifyException(
                        $"Participant '{InputData.Participants[p].name}' has more than one workshop in slot '{InputData.Slots[s]}'.");
                }
            }

            for (int w = 0; w < InputData.Workshops.Count; w++)
            {
                workshopMinConstraint(w);
                workshopMaxConstraint(w);
                conductorsAreInOwnWorkshop(w);
            }

            for (int p = 0; p < InputData.Participants.Count; p++)
            {
                for (int s = 0; s < InputData.Slots.Count; s++)
                {
                    oneWorkshopPerSlot(p, s);
                }
            }
        }
    }
}