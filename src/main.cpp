#include "Types.h"
#include "Util.h"
#include "Version.h"
#include "Options.h"
#include "Status.h"
#include "InputReader.h"
#include "MipSolver.h"
#include "OutputWriter.h"
#include "ConstraintParser.h"

#include <iostream>
#include <fstream>
#include <signal.h>

template<typename Stream>
string readInputStringFromStream(Stream& stream)
{
    std::stringstream res;
    string line;
    while(std::getline(stream, line))
    {
        res << line << '\n';
    }

    return res.str();
}

string readInputString(Options const& options)
{
    if(options.input_files().empty())
    {
        return readInputStringFromStream(std::cin);
    }
    else
    {
        std::stringstream res;
        for(string const& file : options.input_files())
        {
            std::ifstream stream(file);
            if(!stream.is_open())
            {
                throw InputException("File \"" + file + "\" could not be opened.");
            }
            res << readInputStringFromStream(stream) << std::endl;
        }

        return res.str();
    }
}

void output_string(string const& text, string const& fileSuffix, Options const& options)
{
    if(!options.output_file().empty())
    {
        std::ofstream stream(options.output_file() + fileSuffix);
        stream << text;
        stream.close();
    }
    else
    {
        std::cout << text << std::endl << std::endl;
    }
}

void signal_handler(int signal)
{
    (void)signal;
    Status::error("Abort on user request (signal " + str(signal) + ").");
    exit(signal);
}

struct sigaction old_action;
void set_signal_handler(int signal, sighandler_t handler)
{
    struct sigaction action;
    memset(&action, 0, sizeof(action));
    action.sa_handler = handler;
    sigaction(signal, &action, &old_action);
}

int main(int argc, char** argv)
{
    set_signal_handler(SIGINT, signal_handler);
    set_signal_handler(SIGTERM, signal_handler);

    try
    {
        Rng::seed(time_now().time_since_epoch().count());

        const string header = "wsolve [Version " WSOLVE_VERSION "]\n(c) 2020 Maximilian Azendorf\n";
        Options options;

        auto optionsStatus = Options::parse(argc, argv, header, options);

        Status::enable_output(options);

        switch (optionsStatus)
        {
            case OK:
                break;
            case EXIT:
                return 0;
            case ERROR:
            {
                Status::error("Invalid arguments.");
                return 1;
            }
        }

        string inputString = readInputString(options);
        InputData inputData = InputReader::read_input(inputString);
        inputData.build_constraints(ConstraintParser::parse);

        Status::info("Found " + str(inputData.scheduling_constraints().size()) + " scheduling and "
            + str(inputData.assignment_constraints().size()) + " assignment constraints.");

        MipSolver solver(options);
        Solution solution = solver.solve(inputData);

        if (solution.is_invalid())
        {
            Status::info_important("No solution found.");
        }
        else
        {
            Status::info_important("Solution found.");
            Status::info("Solution score: " + Scoring(inputData, options).evaluate(solution).to_str());
            if (inputData.slot_count() > 1)
            {
                string schedulingSolutionString = OutputWriter::write_scheduling_solution(solution);
                output_string(schedulingSolutionString, ".scheduling.csv", options);
            }

            string assignmentSolutionString = OutputWriter::write_assignment_solution(solution);
            output_string(assignmentSolutionString, ".assignment.csv", options);
        }
    }
    catch(InputException const& ex)
    {
        Status::error("Error in input: " + ex.message());
        return 1;
    }

    return 0;
}
