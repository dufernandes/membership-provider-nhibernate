<%@ Application Language="C#" %>

<%@ Import Namespace="NHibernateManager.Helper" %>
<%@ Import Namespace="AutenthicationAuthorization.NHibernateManager" %>

<script runat="server">

    private static NHibernateMembershipMapper _nhibernateMembershipMapper = new NHibernateMembershipMapper();
    private static SessionHelper _sessionHelper = null;

    void Application_Start(object sender, EventArgs e) 
    {
        String[] roles = Roles.GetAllRoles();

        if (roles == null || roles.Length == 0)
        {
            Roles.CreateRole("Administrator");
            Roles.CreateRole("Managers");
            Roles.CreateRole("Personnel");
            Roles.CreateRole("Sales");
            Roles.CreateRole("Marketing");
            Roles.CreateRole("Purchasing");
            Roles.CreateRole("IT");
        }
        else
        {
            CreateRoleIfNotExists("Administrator", roles);
            CreateRoleIfNotExists("Managers", roles);
            CreateRoleIfNotExists("Personnel", roles);
            CreateRoleIfNotExists("Sales", roles);
            CreateRoleIfNotExists("Marketing", roles);
            CreateRoleIfNotExists("Purchasing", roles);
            CreateRoleIfNotExists("IT", roles);
        }
        
        CreateUserIfNotExists("admin", "admin", "Administrator");
        CreateUserIfNotExists("management", "management", "Managers");
        CreateUserIfNotExists("personnel", "personnel", "Personnel");
        CreateUserIfNotExists("sales", "sales", "Sales");
        CreateUserIfNotExists("marketing", "marketing", "Marketing");
        CreateUserIfNotExists("purchasing", "purchasing", "Purchasing");
        CreateUserIfNotExists("it", "it", "IT");
    }

    /// <summary>
    /// Create a user, if he or she does not exist.
    /// </summary>
    /// <param name="userName">User Name - login.</param>
    /// <param name="password">Password.</param>
    /// <param name="role">Role associated with the user to be created. The role must already exist.</param>
    private void CreateUserIfNotExists(String userName, String password, String role)
    {
        MembershipUser user = Membership.GetUser(userName);
        if (user == null)
        {
            Membership.CreateUser(userName, password);
            Roles.AddUsersToRole(new String[] { userName }, role);
        }
    }

    /// <summary>
    /// Create a role if it does not exist in an entered list.
    /// </summary>
    /// <param name="role">Role to be created.</param>
    /// <param name="roles">List to to check whether th role exists.</param>
    private void CreateRoleIfNotExists(String role, String[] roles)
    {
        if (!roles.Contains(role))
        {
            Roles.CreateRole(role);
        }
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        _sessionHelper = SessionManagerFactory.GetSessionHelper(_nhibernateMembershipMapper);
        _sessionHelper.OpenSession();
    }

    void Application_EndRequest(object sender, EventArgs e)
    {
        _sessionHelper.CloseSession();
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
