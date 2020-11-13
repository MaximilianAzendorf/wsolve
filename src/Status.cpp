#include "Status.h"

#include <iostream>

bool Status::_output = false;

const_ptr<Options> Status::_options;

string Status::color(int foregroundColor)
{
    std::stringstream s;
    s << "\033[";
    s << foregroundColor;
    s << "m";

    return s.str();
}

string Status::color_reset()
{
    return "\033[0m";
}

void Status::info(string const& text)
{
    if(_output && _options->verbosity() >= 3)
    {
        std::cerr << "INFO:    " << text << std::endl;
    }
}

void Status::info_important(string const& text)
{
    if(_output && _options->verbosity() >= 1)
    {
        std::cerr << "INFO:    " << text << std::endl;
    }
}

void Status::warning(string const& text)
{
    if(_output && _options->verbosity() >= 2)
    {
        std::cerr << color(33) << "WARNING: " << text << color_reset() << std::endl;
    }
}

void Status::error(string const& text)
{
    if(_output && _options->verbosity() >= 1)
    {
        std::cerr << color(31) << "ERROR:   " << text << color_reset() << std::endl;
    }
}

void Status::enable_output(const_ptr<Options> options)
{
    _options = options;
    _output = true;
}
