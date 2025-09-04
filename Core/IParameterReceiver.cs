namespace ExerciseTracker.Core
{
    public interface IParameterReceiver<TParameter>
    {
        void ReceiveParameter(TParameter parameter);
    }
}
