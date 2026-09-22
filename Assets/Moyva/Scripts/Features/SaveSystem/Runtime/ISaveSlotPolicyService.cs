using Kruty1918.SaveSystem;

namespace Kruty1918.Moyva.SaveSystem
{
    public interface ISaveSlotPolicyService
    {
        bool HasSave(int slot);
        void Delete(int slot);
        SaveSlotInfo GetSlotInfo(int slot);
    }
}
