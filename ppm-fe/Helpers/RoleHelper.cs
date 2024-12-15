namespace ppm_fe.Helpers
{

    public static class RoleHelper
    {
        public static string GetRoleName(int? roleId)
        {
            return roleId switch
            {
                1 => "IT Administrator",
                2 => "Vereinsgründer",
                3 => "Festmitarbeiter",
                4 => "Honorarkraft",
                _ => "Unknown"
            };
        }

        public static int GetRoleId(string? roleName)
        {
            return roleName switch
            {
                "IT Administrator" => 1,
                "Vereinsgründer" => 2,
                "Festmitarbeiter" => 3,
                "Honorarkraft" => 4,
                "Unknown" => 5,
                _ => 5
            };
        }
    }
}
