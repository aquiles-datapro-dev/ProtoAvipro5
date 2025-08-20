using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using Shared.Models;

namespace Shared.Connector;

public partial class CustomDBContext : DbContext
{
    private readonly IConfiguration _configuration;
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<EmployeesFunctionality> EmployeesFunctionalities { get; set; }
    public virtual DbSet<Functionality> Functionalities { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RolesFunctionality> RolesFunctionalities { get; set; }
    public virtual DbSet<State> States { get; set; }
    public virtual DbSet<Vendor> Vendors { get; set; }
    public virtual DbSet<VendorsContact> VendorsContacts { get; set; }

    // Nuevas tablas para autenticación
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<LoginAudit> LoginAudits { get; set; }

    public CustomDBContext()
    {
    }

    public CustomDBContext(DbContextOptions<CustomDBContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DB.MySQL.DefaultConnection");
            optionsBuilder.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        // Configuración de RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refreshtokens");

            entity.HasIndex(e => e.EmployeeId, "fk_refresh_tokens_employees");
            entity.HasIndex(e => e.Token, "ix_refresh_tokens_token").IsUnique();
            entity.HasIndex(e => e.Expires, "ix_refresh_tokens_expires");
            entity.HasIndex(e => e.Revoked, "ix_refresh_tokens_revoked");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Expires)
                .HasColumnType("datetime")
                .HasColumnName("expires");
            entity.Property(e => e.Revoked)
                .HasColumnType("datetime")
                .HasColumnName("revoked")
                .IsRequired(false);
            entity.Property(e => e.RevokedByIp)
                .HasMaxLength(45)
                .HasColumnName("RevokedByIp")
                .IsRequired(false);
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .HasColumnName("CreatedByIP")
                .IsRequired(false);
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("UserAgent")
                .IsRequired(false);
            entity.Property(e => e.ReasonRevoked)
                .HasMaxLength(100)
                .HasColumnName("ReasonRevoked")
                .IsRequired(false);

            entity.HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_refresh_tokens_employees");
        });

        // Configuración de LoginAudit
        modelBuilder.Entity<LoginAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("loginaudits");

            entity.HasIndex(e => e.EmployeeId, "fk_login_audits_employees");
            entity.HasIndex(e => e.LoginTime, "ix_login_audits_login_time");
            entity.HasIndex(e => e.Success, "ix_login_audits_success");
            entity.HasIndex(e => e.IpAddress, "ix_login_audits_ip_address");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmployeeId).HasColumnName("employeeid");
            entity.Property(e => e.LoginTime)
                .HasColumnType("datetime")
                .HasColumnName("logintime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("ipaddress");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("useragent")
                .IsRequired(false);
            entity.Property(e => e.Success)
                .HasColumnName("success")
                .HasDefaultValue(false);
            entity.Property(e => e.FailureReason)
                .HasMaxLength(200)
                .HasColumnName("failurereason")
                .IsRequired(false);

            entity.HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_login_audits_employees");
        });

        // Configuraciones existentes (se mantienen igual)
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("employees");

            entity.HasIndex(e => e.RoleId, "fk_tbl_employees_tbl_roles");

            entity.HasIndex(e => e.StateId, "fk_tbl_employees_tbl_state");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.BalanceVac).HasColumnName("balance_vac");
            entity.Property(e => e.Beeper)
                .HasMaxLength(25)
                .HasColumnName("beeper");
            entity.Property(e => e.Cellular)
                .HasMaxLength(50)
                .HasColumnName("cellular");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateBirth)
                .HasColumnType("datetime")
                .HasColumnName("date_birth");
            entity.Property(e => e.Dep).HasColumnName("dep");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("department");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(20)
                .HasColumnName("emergency_contact");
            entity.Property(e => e.EmergencyPhone)
                .HasMaxLength(25)
                .HasColumnName("emergency_phone");
            entity.Property(e => e.EmergencyRelation)
                .HasMaxLength(25)
                .HasColumnName("emergency_relation");
            entity.Property(e => e.EmpIni)
                .HasMaxLength(5)
                .HasColumnName("emp_ini");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .HasColumnName("emp_mail");
            entity.Property(e => e.FirstName)
                .HasMaxLength(30)
                .HasColumnName("first_name");
            entity.Property(e => e.HireDate)
                .HasColumnType("datetime")
                .HasColumnName("hire_date");
            entity.Property(e => e.HomePhone)
                .HasMaxLength(25)
                .HasColumnName("home_phone");
            entity.Property(e => e.Insp).HasColumnName("insp");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.LastName)
                .HasMaxLength(30)
                .HasColumnName("last_name");
            entity.Property(e => e.LastWorkDate)
                .HasColumnType("datetime")
                .HasColumnName("last_work_date");
            entity.Property(e => e.MS)
                .HasMaxLength(1)
                .HasColumnName("m_s");
            entity.Property(e => e.MiddleInitial)
                .HasMaxLength(3)
                .HasColumnName("middle_initial");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.OfficePersonal).HasColumnName("office_personal");
            entity.Property(e => e.Password)
                .HasColumnType("text")
                .HasColumnName("password");
            entity.Property(e => e.Ri).HasColumnName("ri");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Rts).HasColumnName("rts");
            entity.Property(e => e.Service).HasColumnName("service");
            entity.Property(e => e.Shop)
                .HasMaxLength(20)
                .HasColumnName("shop");
            entity.Property(e => e.SingOffTitle)
                .HasMaxLength(50)
                .HasColumnName("sing_off_title");
            entity.Property(e => e.Ss)
                .HasMaxLength(15)
                .HasColumnName("ss");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.Tech).HasColumnName("tech");
            entity.Property(e => e.Title)
                .HasMaxLength(40)
                .HasColumnName("title");
            entity.Property(e => e.TotalVac).HasColumnName("total_vac");
            entity.Property(e => e.UsedVac).HasColumnName("used_vac");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
            entity.Property(e => e.Wages)
                .HasPrecision(19, 4)
                .HasColumnName("wages");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(15)
                .HasColumnName("zip_code");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_tbl_employees_tbl_roles");

            entity.HasOne(d => d.State).WithMany(p => p.Employees)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("fk_tbl_employees_tbl_state");
        });

        modelBuilder.Entity<EmployeesFunctionality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("employees_functionalities");

            entity.HasIndex(e => e.EmployeeId, "fk_employees_functionalities_employees");

            entity.HasIndex(e => e.FunctionalityId, "fk_employees_functionalities_functionalities");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FunctionalityId).HasColumnName("functionality_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeesFunctionalities)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_functionalities_employees");

            entity.HasOne(d => d.Functionality).WithMany(p => p.EmployeesFunctionalities)
                .HasForeignKey(d => d.FunctionalityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_functionalities_functionalities");
        });

        modelBuilder.Entity<Functionality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("functionalities");

            entity.HasIndex(e => e.ModuleId, "fk_functionalities_module");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");

            entity.HasOne(d => d.Module).WithMany(p => p.Functionalities)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_functionalities_module");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("module");

            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.HasIndex(e => e.ParentRoleId, "fk_tbl_roles_tbl_roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ParentRoleId).HasColumnName("parent_role_id");

            entity.HasOne(d => d.ParentRole).WithMany(p => p.InverseParentRole)
                .HasForeignKey(d => d.ParentRoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_tbl_roles_tbl_roles");
        });

        modelBuilder.Entity<RolesFunctionality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles_functionalities");

            entity.HasIndex(e => e.FunctionalityId, "fk_roles_functionalities_functionalities");

            entity.HasIndex(e => e.RoleId, "fk_roles_functionalities_roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FunctionalityId).HasColumnName("functionality_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Functionality).WithMany(p => p.RolesFunctionalities)
                .HasForeignKey(d => d.FunctionalityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_roles_functionalities_functionalities");

            entity.HasOne(d => d.Role).WithMany(p => p.RolesFunctionalities)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_roles_functionalities_roles");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("state");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Abbr)
                .HasMaxLength(50)
                .HasColumnName("abbr");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("vendors");

            entity.HasIndex(e => e.StateId, "fk_tbl_vendors_tbl_state");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcctNumber)
                .HasMaxLength(50)
                .HasColumnName("acct_number");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.AuditNotes)
                .HasColumnType("text")
                .HasColumnName("audit_notes");
            entity.Property(e => e.AuditVendor).HasColumnName("audit_vendor");
            entity.Property(e => e.BalDue)
                .HasPrecision(19, 4)
                .HasColumnName("bal_due");
            entity.Property(e => e.BillAdd1)
                .HasMaxLength(50)
                .HasColumnName("bill_add_1");
            entity.Property(e => e.BillAdd2)
                .HasMaxLength(50)
                .HasColumnName("bill_add_2");
            entity.Property(e => e.BillCity)
                .HasMaxLength(50)
                .HasColumnName("bill_city");
            entity.Property(e => e.BillCountry)
                .HasMaxLength(50)
                .HasColumnName("bill_country");
            entity.Property(e => e.BillState)
                .HasMaxLength(2)
                .HasColumnName("bill_state");
            entity.Property(e => e.BillZip)
                .HasMaxLength(50)
                .HasColumnName("bill_zip");
            entity.Property(e => e.Blacklist).HasColumnName("blacklist");
            entity.Property(e => e.City)
                .HasMaxLength(25)
                .HasColumnName("city");
            entity.Property(e => e.Contact)
                .HasMaxLength(100)
                .HasColumnName("contact");
            entity.Property(e => e.ContactSecond)
                .HasMaxLength(100)
                .HasColumnName("contact_second");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateAuditSent)
                .HasColumnType("datetime")
                .HasColumnName("date_audit_sent");
            entity.Property(e => e.DrugProDate)
                .HasColumnType("datetime")
                .HasColumnName("drug_pro_date");
            entity.Property(e => e.EasaDate)
                .HasColumnType("datetime")
                .HasColumnName("easa_date");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FastType).HasColumnName("fast_type");
            entity.Property(e => e.Fax)
                .HasMaxLength(25)
                .HasColumnName("fax");
            entity.Property(e => e.InternetAddress)
                .HasMaxLength(50)
                .HasColumnName("internet_address");
            entity.Property(e => e.IsoDate)
                .HasColumnType("datetime")
                .HasColumnName("iso_date");
            entity.Property(e => e.LastAuditDate)
                .HasColumnType("datetime")
                .HasColumnName("last_audit_date");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.NapCapDate)
                .HasColumnType("datetime")
                .HasColumnName("nap_cap_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Phone)
                .HasMaxLength(25)
                .HasColumnName("phone");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.Terms).HasColumnName("terms");
            entity.Property(e => e.VendorName)
                .HasMaxLength(50)
                .HasColumnName("vendor_name");
            entity.Property(e => e.VendorTax).HasColumnName("vendor_tax");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(15)
                .HasColumnName("zip_code");

            entity.HasOne(d => d.State).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("fk_tbl_vendors_tbl_state");
        });

        modelBuilder.Entity<VendorsContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("vendors_contact");

            entity.HasIndex(e => e.VendorId, "fk_tbl_vendors_contact_tbl_vendors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasColumnName("fax");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");

            entity.HasOne(d => d.Vendor).WithMany(p => p.VendorsContacts)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("fk_tbl_vendors_contact_tbl_vendors");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}