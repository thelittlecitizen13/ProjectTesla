using System.Text;

namespace TeslaCommon
{
    public interface IMember
    {
        string Name { get; set; }
        IMemberData Data { get; set; }
    }
}
