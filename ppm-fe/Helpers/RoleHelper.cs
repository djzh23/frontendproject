﻿namespace ppm_fe.Helpers
{

    public static class RoleHelper
    {
        public static string GetRoleName(int? roleId)
        {
            return roleId switch
            {
                1 => "IT Administrator",
                2 => "Projekt-Koordinator",
                3 => "Festmitarbeiter",
                4 => "Honorarkraft",
                _ => "Unbekannt"
            };
        }

        public static int GetRoleId(string? roleName)
        {
            return roleName switch
            {
                "IT Administrator" => 1,
                "Projekt-Koordinator" => 2,
                "Festmitarbeiter" => 3,
                "Honorarkraft" => 4,
                "Unbekannt" => 5,
                _ => 5
            };
        }
    }
}
