namespace WSolve.ExtraConditions {
    public class ParticipantAccessorBase 
    {
        protected readonly ExtraConditionsBase _base;
        protected readonly Chromosome Chromosome;
        protected readonly int _id;

        public ParticipantAccessorBase(int id, ExtraConditionsBase @base, Chromosome chromosome)
        {
            _id = id;
            _base = @base;
            Chromosome = chromosome;
        }

        public string Name => Chromosome.InputData.Participants[_id].name;
        internal int Id => _id;

        public static bool operator ==(ParticipantAccessorBase left, ParticipantAccessorBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ParticipantAccessorBase left, ParticipantAccessorBase right)
        {
            return !Equals(left, right);
        }

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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ParticipantAccessorBase) obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        protected bool Equals(ParticipantAccessorBase other)
        {
            return _id == other._id;
        }
    }
}