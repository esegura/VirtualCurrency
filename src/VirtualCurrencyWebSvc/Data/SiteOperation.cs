namespace VirtualCurrencyWebSvc.Data
{
    // Keep these operations in sync with all the web pages used to send values to the server
    public enum SiteOperation
    {
        // session-related operations
        ValidateLoginToken,

        // user-related operations
        CreateUser,
        ChangePassword,

        // role-related operations
        CreateRole,
        ListRoles,          // returns *all* roles of the system, regardless of owner or type. Caller must have permission to call this.
        ListRolesForUser,   // returns the roles for a given user. Caller must have permission to call this.
        LinkRoleAndUser,
        UnlinkRoleAndUser,

        // resource-related operations
        ListResources,      // returns *all* resources of the system, regardless of owner or type. Caller must have permission to call this.
        AddPermission,
        RemovePermission,

        // misc
        ListUsersAndRolesAndResources,
        ListAllUsersWithTheirRoles,

        //conversion tools
        ExportToPowerpoint,
        ExportToPdf,
        ConvertExcelToJson,

        // file-related operations
        List,
        Load,
        Save,
        Delete,
        Move,
        SaveImage,
        LoadImage,
        LoadResizedImage,
        LoadJsonWithTitle,
        LoadImageWithTitle,
        LoadResizedImageWithTitle,
        SaveAsOtherUser,
        PublishToRole
    }
}
