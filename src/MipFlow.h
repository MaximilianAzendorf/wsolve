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

#pragma once

#include "Types.h"

#include <ortools/linear_solver/linear_solver.h>

namespace op = operations_research;

template<typename NodeKey, typename EdgeKey>
class MipFlow
{
private:
    map<NodeKey, int> _nodeMap;
    map<EdgeKey, int> _edgeMap;
    vector<int> _supply;
    vector<vector<int>> _outgoing;
    vector<vector<int>> _incoming;
    vector<int> _edgesMax;
    vector<long> _edgesCost;
    vector<vector<int>> _edgeGroups;
    vector<int> _blockedEdges;
    vector<int> _solution;

public:
    int add_node();

    int add_node(NodeKey key);

    void add_supply(int node, int supply);

    int add_edge(int fromNode, int toNode, int max, long unitCost);

    int add_edge(EdgeKey key, int fromNode, int toNode, int max, long unitCost);

    template<typename EdgeKeyIterator>
    void create_edge_group_or_block_edges(EdgeKeyIterator begin, EdgeKeyIterator end);

    bool solve(op::MPSolver& solver);

    [[nodiscard]] int solution_value_at(EdgeKey key) const;

    [[nodiscard]] map<NodeKey, int> const& nodes() const;

    [[nodiscard]] map<EdgeKey, int> const& edges() const;

    [[nodiscard]] int node_count() const;

    [[nodiscard]] int edge_count() const;
};

#include "MipFlow.ipp"