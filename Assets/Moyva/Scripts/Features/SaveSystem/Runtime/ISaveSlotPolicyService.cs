namespace Kruty1918.Moyva.SaveSystem
{
    internal interface ISaveSlotPolicyService
    {
        bool HasSave(int slot);
        void Delete(int slot);
        SaveSlotInfo GetSlotInfo(int slot);
    }
}
