using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TMWeb.EFModels;

public partial class TmwebContext : DbContext
{
    public TmwebContext()
    {
    }

    public TmwebContext(DbContextOptions<TmwebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ItemDetail> ItemDetails { get; set; }

    public virtual DbSet<ItemRecordConfig> ItemRecordConfigs { get; set; }

    public virtual DbSet<ItemRecordContent> ItemRecordContents { get; set; }

    public virtual DbSet<ItemRecordDetail> ItemRecordDetails { get; set; }

    public virtual DbSet<Machine> Machines { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagCategory> TagCategories { get; set; }

    public virtual DbSet<TaskDetail> TaskDetails { get; set; }

    public virtual DbSet<TaskRecordConfig> TaskRecordConfigs { get; set; }

    public virtual DbSet<TaskRecordContent> TaskRecordContents { get; set; }

    public virtual DbSet<TaskRecordDetail> TaskRecordDetails { get; set; }

    public virtual DbSet<Workerder> Workerders { get; set; }

    public virtual DbSet<WorkorderRecipeConfig> WorkorderRecipeConfigs { get; set; }

    public virtual DbSet<WorkorderRecipeContent> WorkorderRecipeContents { get; set; }

    public virtual DbSet<WorkorderRecordConfig> WorkorderRecordConfigs { get; set; }

    public virtual DbSet<WorkorderRecordContent> WorkorderRecordContents { get; set; }

    public virtual DbSet<WorkorderRecordDetail> WorkorderRecordDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=TMWeb;Trusted_Connection=True; trustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemDeta__3214EC27F6684215");

            entity.HasIndex(e => new { e.WorkordersId, e.SerialNo }, "ItemDetails_index_1").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.Ngamount).HasColumnName("NGAmount");
            entity.Property(e => e.Okamount).HasColumnName("OKAmount");
            entity.Property(e => e.SerialNo).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.WorkordersId).HasColumnName("WorkordersID");

            entity.HasOne(d => d.Workorders).WithMany(p => p.ItemDetails)
                .HasForeignKey(d => d.WorkordersId)
                .HasConstraintName("FK__ItemDetai__Worko__52593CB8");
        });

        modelBuilder.Entity<ItemRecordConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemReco__3214EC27CFDC227D");

            entity.HasIndex(e => e.ItemRecordCategory, "UQ__ItemReco__18745CFC9C0D40E6").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ItemRecordCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<ItemRecordContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemReco__3214EC2781072052");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecordName).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.ItemRecordContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__ItemRecor__Confi__4BAC3F29");
        });

        modelBuilder.Entity<ItemRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.ItemId, e.RecordContentId }).HasName("PK__ItemReco__68A347182D4036A7");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.ItemRecordDetails)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ItemRecor__ItemI__4CA06362");

            entity.HasOne(d => d.RecordContent).WithMany(p => p.ItemRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ItemRecor__Recor__4D94879B");
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Machine__3214EC274421D01E");

            entity.ToTable("Machine");

            entity.HasIndex(e => e.Name, "UQ__Machine__737584F66D3A72BE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.TagCategoryId).HasColumnName("TagCategoryID");

            entity.HasOne(d => d.TagCategory).WithMany(p => p.Machines)
                .HasForeignKey(d => d.TagCategoryId)
                .HasConstraintName("FK__Machine__TagCate__6477ECF3");
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Procress__3214EC278A309CA1");

            entity.ToTable("Process");

            entity.HasIndex(e => e.Name, "UQ__Procress__737584F65C4A7E94").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stations__3214EC270BCF7AA2");

            entity.HasIndex(e => e.Name, "UQ__Stations__737584F6F3E14589").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");

            entity.HasOne(d => d.Process).WithMany(p => p.Stations)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("FK__Stations__Proces__7A672E12");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tag__3214EC278708C4AC");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "UQ__Tag__737584F6B79A56E3").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Param1).HasMaxLength(50);
            entity.Property(e => e.Param2).HasMaxLength(50);
            entity.Property(e => e.Param3).HasMaxLength(50);
            entity.Property(e => e.Param4).HasMaxLength(50);
            entity.Property(e => e.Param5).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Tags)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Tag__CategoryID__656C112C");
        });

        modelBuilder.Entity<TagCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TagCateg__3214EC27752950F3");

            entity.ToTable("TagCategory");

            entity.HasIndex(e => e.Name, "UQ__TagCateg__737584F698D4876C").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskDeta__3214EC27347C5450");

            entity.HasIndex(e => new { e.ItemId, e.StationId, e.SerialNo }, "TaskDetails_index_2").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Ngamount).HasColumnName("NGAmount");
            entity.Property(e => e.Okamount).HasColumnName("OKAmount");
            entity.Property(e => e.SerialNo).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StationId).HasColumnName("StationID");

            entity.HasOne(d => d.Item).WithMany(p => p.TaskDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__TaskDetai__ItemI__534D60F1");

            entity.HasOne(d => d.Station).WithMany(p => p.TaskDetails)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK__TaskDetai__Stati__3F115E1A");
        });

        modelBuilder.Entity<TaskRecordConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskReco__3214EC2726CE366B");

            entity.HasIndex(e => e.TaskRecordsCategory, "UQ__TaskReco__C644E696E488BD53").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.TaskRecordsCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskRecordContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskReco__3214EC27FBA4FD56");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecordName).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.TaskRecordContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__TaskRecor__Confi__4F7CD00D");
        });

        modelBuilder.Entity<TaskRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.RecordContentId }).HasName("PK__TaskReco__66B48D228D2A2DFA");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.RecordContent).WithMany(p => p.TaskRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskRecor__Recor__5165187F");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskRecordDetails)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskRecor__TaskI__5070F446");
        });

        modelBuilder.Entity<Workerder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workerde__3214EC27B578A3E9");

            entity.HasIndex(e => new { e.WorkerderNo, e.Lot }, "Workerders_index_0").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.ItemRecordsCategoryId).HasColumnName("ItemRecordsCategoryID");
            entity.Property(e => e.Lot).HasMaxLength(50);
            entity.Property(e => e.Ngamount).HasColumnName("NGAmount");
            entity.Property(e => e.Okamount).HasColumnName("OKAmount");
            entity.Property(e => e.PartNo).HasMaxLength(50);
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.RecipeCategoryId).HasColumnName("RecipeCategoryID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.TaskRecordCategoryId).HasColumnName("TaskRecordCategoryID");
            entity.Property(e => e.WorkerderNo).HasMaxLength(50);
            entity.Property(e => e.WorkorderRecordCategoryId).HasColumnName("WorkorderRecordCategoryID");

            entity.HasOne(d => d.ItemRecordsCategory).WithMany(p => p.Workerders)
                .HasForeignKey(d => d.ItemRecordsCategoryId)
                .HasConstraintName("FK__Workerder__ItemR__4AB81AF0");

            entity.HasOne(d => d.Process).WithMany(p => p.Workerders)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("FK__Workerder__Proce__02FC7413");

            entity.HasOne(d => d.RecipeCategory).WithMany(p => p.Workerders)
                .HasForeignKey(d => d.RecipeCategoryId)
                .HasConstraintName("FK__Workerder__Recip__4316F928");

            entity.HasOne(d => d.TaskRecordCategory).WithMany(p => p.Workerders)
                .HasForeignKey(d => d.TaskRecordCategoryId)
                .HasConstraintName("FK__Workerder__TaskR__4E88ABD4");

            entity.HasOne(d => d.WorkorderRecordCategory).WithMany(p => p.Workerders)
                .HasForeignKey(d => d.WorkorderRecordCategoryId)
                .HasConstraintName("FK__Workerder__Worko__46E78A0C");
        });

        modelBuilder.Entity<WorkorderRecipeConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC2767255ACB");

            entity.HasIndex(e => e.RecipeCategory, "UQ__Workorde__46E6E02CA09C21CA").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.RecipeCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkorderRecipeContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC2797B01E46");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecipeName).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.WorkorderRecipeContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__Workorder__Confi__440B1D61");
        });

        modelBuilder.Entity<WorkorderRecordConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC277B93F476");

            entity.HasIndex(e => e.WorkorderRecordCategory, "UQ__Workorde__95EF011499FA8575").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.WorkorderRecordCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkorderRecordContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC27FE1A4E8A");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecordName).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.WorkorderRecordContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__Workorder__Confi__47DBAE45");
        });

        modelBuilder.Entity<WorkorderRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.WorkerderId, e.RecordContentId }).HasName("PK__Workorde__E4F8D8DCF8CBC459");

            entity.Property(e => e.WorkerderId).HasColumnName("WorkerderID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.RecordContent).WithMany(p => p.WorkorderRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workorder__Recor__49C3F6B7");

            entity.HasOne(d => d.Workerder).WithMany(p => p.WorkorderRecordDetails)
                .HasForeignKey(d => d.WorkerderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workorder__Worke__48CFD27E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
