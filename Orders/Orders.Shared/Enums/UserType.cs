using System.ComponentModel;

namespace Orders.Shared.Enums
{
    public enum UserType
    {
        [Description("Administrador")]
        Admin,

        [Description("Usuario")]
        User
    }
}