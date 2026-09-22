namespace Kruty1918.SaveSystem
{
    public interface ISaveSlotPolicyService
    {
        bool HasSave(int slot);
        void Delete(int slot);
        SaveSlotInfo GetSlotInfo(int slot);
    }
}
