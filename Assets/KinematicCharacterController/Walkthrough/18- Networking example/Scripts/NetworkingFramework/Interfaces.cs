
namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public interface IEntity
    {
        int GetId();
        void SetId(int id);
        bool GetHasOwnership();
        void SetHasOwnership(bool hasOwnership);
    }

    public interface IPlayerCommands
    {
        void WriteToBuffer(SerializationBuffer buf, bool mustBeFullState);
        void ReadFromBuffer(SerializationBuffer buf);
    }

    public interface IWorldSnapshot<WS>
    {
        void WriteToBuffer(SerializationBuffer buf, bool mustBeFullState);
        void ReadFromBuffer(SerializationBuffer buf);
    }

    public interface IPlayerController<CMD> 
        where CMD : struct, IPlayerCommands
    {
        int GetId();
        void SetId(int id);
        int GetConnectionId();
        void SetConnectionId(int id);
        bool GetIsLocal();
        void SetIsLocal(bool local);

        CMD GetCommands();
        void SetCommands(CMD playerInputs);
        CMD GetNewCommands(CMD previousInputs);

        void UpdateLocalCommands(float deltaTime);
        void ApplyCommands(bool allowEvents);
    }

    public interface ISimulationController<WS>
        where WS : struct, IWorldSnapshot<WS>
    {
        void SimulateStep(float deltaTime);

        void InitializeSnapshot(ref WS worldSnapshot);
        void SaveStateToSnapshot(ref WS worldSnapshot);
        void RestoreStateFromSnapshot(ref WS worldSnapshot);
        
        void OnConnect(int connectionId);
        void OnDisconnect(int connectionId);
        void OnData(int connectionId, int channelId, int receivedSize, int msgId);

        void OnPreSnapshot();
        void OnPostSnapshot();
    }

    public interface ISimulationEvent
    {
        void Apply();
    }
}