namespace WSolve
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class CustomFilter
    {
        private readonly Chromosome _chromosome;

        protected CustomFilter(Chromosome chromosome)
        {
            _chromosome = chromosome;
        }
        
        public bool Result { get; private set; } = true;

        protected IEnumerable<ParticipantAccessor> Participants => Enumerable.Range(0, _chromosome.InputData.Participants.Count).Select(n => new ParticipantAccessor(n, this, _chromosome));
        
        protected IEnumerable<WorkshopAccessor> Workshops => Enumerable.Range(0, _chromosome.InputData.Workshops.Count).Select(n => new WorkshopAccessor(n, this, _chromosome));
        
        protected IEnumerable<SlotAccessor> Slots => Enumerable.Range(0, _chromosome.InputData.Slots.Count).Select(n => new SlotAccessor(n, this, _chromosome));

        protected void AddCondition(bool condition)
        {
            Result &= condition;
        }
        
        protected ParticipantAccessor Participant(string nameFragment)
        {
            int res = _chromosome.InputData.Participants.FindIndex(p => p.name == nameFragment);
            if (res >= 0)
            {
                return new ParticipantAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Participants.FindIndex(p => p.name.StartsWith(nameFragment));
            if (res >= 0)
            {
                return new ParticipantAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Participants.FindIndex(p => p.name.Contains(nameFragment));
            if (res >= 0)
            {
                return new ParticipantAccessor(res, this, _chromosome);
            }

            throw new ArgumentException($"Could not find participant with name fragment '{nameFragment}'.");
        }
        
        protected WorkshopAccessor Workshop(string nameFragment)
        {
            int res = _chromosome.InputData.Workshops.FindIndex(w => w.name == nameFragment);
            if (res >= 0)
            {
                return new WorkshopAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Workshops.FindIndex(w => w.name.StartsWith(nameFragment));
            if (res >= 0)
            {
                return new WorkshopAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Workshops.FindIndex(w => w.name.Contains(nameFragment));
            if (res >= 0)
            {
                return new WorkshopAccessor(res, this, _chromosome);
            }

            throw new ArgumentException($"Could not find workshop with name fragment '{nameFragment}'.");
        }
        
        protected SlotAccessor Slot(string nameFragment)
        {
            int res = _chromosome.InputData.Slots.FindIndex(s => s == nameFragment);
            if (res >= 0)
            {
                return new SlotAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Slots.FindIndex(s => s.StartsWith(nameFragment));
            if (res >= 0)
            {
                return new SlotAccessor(res, this, _chromosome);
            }

            res = _chromosome.InputData.Slots.FindIndex(s => s.Contains(nameFragment));
            if (res >= 0)
            {
                return new SlotAccessor(res, this, _chromosome);
            }

            throw new ArgumentException($"Could not find slot with name fragment '{nameFragment}'.");
        }

        public class ParticipantAccessor
        {
            private readonly int _id;
            private readonly CustomFilter _base;
            private readonly Chromosome _chromosome;
            
            public ParticipantAccessor(int id, CustomFilter @base, Chromosome chromosome)
            {
                _id = id;
                _base = @base;
                _chromosome = chromosome;
            }
            
            public string Name => _chromosome.InputData.Participants[_id].name;
            
            public IEnumerable<WorkshopAccessor> Workshops => Enumerable.Range(0, _chromosome.InputData.Slots.Count)
                .Select(n => _chromosome.Workshop(_id, n)).Select(w => new WorkshopAccessor(w, _base, _chromosome));
            
            public static bool operator ==(ParticipantAccessor left, ParticipantAccessor right)
            {
                return Equals(left, right);
            }
            
            public static bool operator !=(ParticipantAccessor left, ParticipantAccessor right)
            {
                return !Equals(left, right);
            }

            public static bool operator ==(ParticipantAccessor left, string right) => left == left?._base.Participant(right);
            
            public static bool operator ==(string left, ParticipantAccessor right) => right == right?._base.Participant(left);
            
            public static bool operator !=(ParticipantAccessor left, string right) => !(left == right);
            
            public static bool operator !=(string left, ParticipantAccessor right) => !(left == right);
            
            public WorkshopAccessor WorkshopAt(SlotAccessor slot) => Workshops.Single(w => w.Slot == slot);
            
            public WorkshopAccessor WorkshopAt(string slotNameFragment) => WorkshopAt(_base.Slot(slotNameFragment));
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj is string s)
                {
                    return this == s;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((ParticipantAccessor) obj);
            }

            public override int GetHashCode()
            {
                return _id;
            }
            
            protected bool Equals(ParticipantAccessor other)
            {
                return _id == other._id;
            }
        }

        public class WorkshopAccessor
        {
            private readonly int _id;
            private readonly CustomFilter _base;
            private readonly Chromosome _chromosome;
            
            public WorkshopAccessor(int id, CustomFilter @base, Chromosome chromosome)
            {
                _id = id;
                _base = @base;
                _chromosome = chromosome;
            }
            
            public IEnumerable<ParticipantAccessor> Participants
            {
                get
                {
                    for (int p = 0; p < _chromosome.InputData.Participants.Count; p++)
                    {
                        for (int n = 0; n < _chromosome.InputData.Slots.Count; n++)
                        {
                            if (_chromosome.Workshop(p, n) == _id)
                            {
                                yield return new ParticipantAccessor(p, _base, _chromosome);
                            }
                        }
                    }
                }
            }

            public IEnumerable<ParticipantAccessor> Conductors => _chromosome.InputData.Workshops[_id].conductors
                .Select(n => new ParticipantAccessor(n, _base, _chromosome));
            
            public SlotAccessor Slot => new SlotAccessor(_chromosome.Slot(_id), _base, _chromosome);
            
            public string Name => _chromosome.InputData.Workshops[_id].name;
            
            public int MinParticipants => _chromosome.InputData.Workshops[_id].min;
            
            public int MaxParticipants => _chromosome.InputData.Workshops[_id].max;
            
            public static bool operator ==(WorkshopAccessor left, WorkshopAccessor right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(WorkshopAccessor left, WorkshopAccessor right)
            {
                return !Equals(left, right);
            }

            public static bool operator ==(WorkshopAccessor left, string right) => left == left?._base.Workshop(right);
            
            public static bool operator ==(string left, WorkshopAccessor right) => right == right?._base.Workshop(left);
            
            public static bool operator !=(WorkshopAccessor left, string right) => !(left == right);
            
            public static bool operator !=(string left, WorkshopAccessor right) => !(left == right);
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj is string s)
                {
                    return this == s;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((WorkshopAccessor) obj);
            }

            public override int GetHashCode()
            {
                return _id;
            }
            
            protected bool Equals(WorkshopAccessor other)
            {
                return _id == other._id;
            }
        }

        public class SlotAccessor
        {
            private readonly int _id;
            private readonly CustomFilter _base;
            private readonly Chromosome _chromosome;
            
            public SlotAccessor(int id, CustomFilter @base, Chromosome chromosome)
            {
                _id = id;
                _base = @base;
                _chromosome = chromosome;
            }
            
            public IEnumerable<WorkshopAccessor> Workshops => Enumerable.Range(0, _chromosome.InputData.Workshops.Count)
                .Where(w => _chromosome.Slot(w) == _id).Select(w => new WorkshopAccessor(w, _base, _chromosome));
            
            public string Name => _chromosome.InputData.Slots[_id];
            
            public static bool operator ==(SlotAccessor left, SlotAccessor right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(SlotAccessor left, SlotAccessor right)
            {
                return !Equals(left, right);
            }
            
            public static bool operator ==(SlotAccessor left, string right) => left == left?._base.Slot(right);
            
            public static bool operator ==(string left, SlotAccessor right) => right == right?._base.Slot(left);
            
            public static bool operator !=(SlotAccessor left, string right) => !(left == right);
            
            public static bool operator !=(string left, SlotAccessor right) => !(left == right);
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj is string s)
                {
                    return this == s;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((SlotAccessor) obj);
            }

            public override int GetHashCode()
            {
                return _id;
            }
            
            private bool Equals(SlotAccessor other)
            {
                return _id == other._id;
            }
        }
    }
}