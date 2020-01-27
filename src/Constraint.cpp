#include "Constraint.h"

Constraint::Constraint(ConstraintType type, int left, int right, int extra)
        : _type(type), _left(left), _right(right), _extra(extra)
{
}

Constraint Constraint::negation()
{
    Constraint neg = *this;
    switch(_type)
    {
        case WorkshopIsInSlot: neg._type = WorkshopIsNotInSlot; break;
        case WorkshopIsNotInSlot: neg._type = WorkshopIsInSlot; break;
        case WorkshopsAreInSameSlot: neg._type = WorkshopsAreNotInSameSlot; break;
        case WorkshopsAreNotInSameSlot: neg._type = WorkshopsAreInSameSlot; break;
        case SlotHasLimitedSize: neg._extra = -neg._extra; break;
        case SlotContainsWorkshop: neg._type = SlotNotContainsWorkshop; break;
        case SlotNotContainsWorkshop: neg._type = SlotContainsWorkshop; break;

        case ParticipantIsInWorkshop: neg._type = ParticipantIsNotInWorkshop; break;
        case ParticipantIsNotInWorkshop: neg._type = ParticipantIsInWorkshop; break;
        case WorkshopContainsParticipant: neg._type = WorkshopNotContainsParticipant; break;
        case WorkshopNotContainsParticipant: neg._type = WorkshopContainsParticipant; break;

            // Constraints with no valid negation
        case Invalid:
        case WorkshopsHaveOffset:
        case SlotsHaveSameWorkshops:
        case WorkshopsHaveSameParticipants:
        case ParticipantsHaveSameWorkshops:
            neg._type = Invalid;
            break;
    }

    return neg;
}

ConstraintType Constraint::type() const
{
    return _type;
}

int Constraint::left() const
{
    return _left;
}

int Constraint::right() const
{
    return _right;
}

int Constraint::extra() const
{
    return _extra;
}

bool Constraint::is_valid() const
{
    return _type != Invalid;
}

bool Constraint::is_scheduling_constraint() const
{
    return _type < CONSTRAINT_TYPE_DISCRIMINATION_LIMIT && _type != Invalid;
}

bool Constraint::is_assignment_constraint() const
{
    return _type >= CONSTRAINT_TYPE_DISCRIMINATION_LIMIT && _type != Invalid;
}

bool Constraint::operator==(Constraint const& other) const
{
    return _type == other._type && _left == other._left && _right == other._right && _extra == other._extra;
}

size_t hash_value(Constraint const& c)
{
    int hash = 0;
    hash = hash * 101 + c.left();
    hash = hash * 101 + c.right();
    hash = hash * 101 + c.extra();
    hash = hash * 101 + c.type();
    return hash;
}
