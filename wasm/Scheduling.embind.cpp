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

#include "../src/Scheduling.h"

#include <emscripten/bind.h>
using namespace emscripten;

EMSCRIPTEN_BINDINGS(wassign_scheduling)
{
    class_<Scheduling>("Scheduling")
            .smart_ptr<const_ptr<Scheduling>>("Scheduling")
            .property("isFeasible", &Scheduling::is_feasible)
            .function("setOf", &Scheduling::set_of)
            .property("inputData", &Scheduling::input_data);
};