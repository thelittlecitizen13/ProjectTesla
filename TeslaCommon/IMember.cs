using System.Text;

namespace TeslaCommon
{
    public interface IMember
    {
        string Name { get; set; }
        MemberData Data { get; set; }
    }
}
