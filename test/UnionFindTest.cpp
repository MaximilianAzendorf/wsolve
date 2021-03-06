/*
 * Copyright 2020 Maximilian Azendorf
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include "common.h"

#include "../src/UnionFind.h"

#define PREFIX "[Union-Find] "

TEST_CASE(PREFIX "Union find constructor works")
{
    REQUIRE(UnionFind<int>(0).size() == 0);
    REQUIRE(UnionFind<int>(1).size() == 1);
    REQUIRE(UnionFind<int>(5).size() == 5);

    REQUIRE_THROWS(UnionFind<int>(-1).size() == 5);
}

TEST_CASE(PREFIX "Union find works")
{
    const int MAX = 4;
    UnionFind<int> uf(MAX);

    SECTION("Initial state is correct (find)")
    {
        for(int i = 0; i < MAX; i++)
        {
            for(int j = i + 1; j < MAX; j++)
            {
                REQUIRE(uf.find(i) != uf.find(j));
            }
        }
    }

    SECTION("Initial state is correct (groups)")
    {
        vector<bool> found(MAX);
        auto groups = uf.groups();

        for(int i = 0; i < MAX; i++)
        {
            REQUIRE(groups[i].size() == 1);
            REQUIRE(!found[groups[i].front()]);

            found[groups[i].front()] = true;
        }
    }

    SECTION("Join works")
    {
        uf.join(0, 1);
        uf.join(1, 3);
        REQUIRE(uf.find(0) == uf.find(1));
        REQUIRE(uf.find(1) == uf.find(3));
        REQUIRE(uf.find(1) != uf.find(2));

        bool foundJoinedGroup = false;
        for(auto group : uf.groups())
        {
            if(group.size() == 3)
            {
                REQUIRE(!foundJoinedGroup);
                foundJoinedGroup = true;
                std::sort(group.begin(), group.end());
                REQUIRE(group[0] == 0);
                REQUIRE(group[1] == 1);
                REQUIRE(group[2] == 3);
            }
        }

        REQUIRE(foundJoinedGroup);
    }
}