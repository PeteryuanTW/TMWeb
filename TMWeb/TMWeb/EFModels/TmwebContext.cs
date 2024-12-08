using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CommonLibrary.MachinePKG.EFModel;

namespace TMWeb.EFModels;

public partial class TmwebContext : DbContext
{
    public TmwebContext(DbContextOptions<TmwebContext> options)
        : base(options)
    {
    }


    public virtual DbSet<ItemDetail> ItemDetails { get; set; }

    public virtual DbSet<ItemRecordConfig> ItemRecordConfigs { get; set; }

    public virtual DbSet<ItemRecordContent> ItemRecordContents { get; set; }

    public virtual DbSet<ItemRecordDetail> ItemRecordDetails { get; set; }

    //public virtual DbSet<MachineBase> MachineBases { get; set; }
    //public virtual DbSet<Machine> Machines { get; set; }

    public virtual DbSet<MachineStatusLog> MachineStatusLogs { get; set; }

    public virtual DbSet<MapComponent> MapComponents { get; set; }

    public virtual DbSet<MapConfig> MapConfigs { get; set; }

    public virtual DbSet<MapImage> MapImages { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    public virtual DbSet<ProcessMachineRelation> ProcessMachineRelations { get; set; }

    public virtual DbSet<ScriptConfig> ScriptConfigs { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<StationUirecord> StationUirecords { get; set; }

    //public virtual DbSet<Tag> Tags { get; set; }

    //public virtual DbSet<TagCategory> TagCategories { get; set; }

    public virtual DbSet<TaskDetail> TaskDetails { get; set; }

    //public virtual DbSet<LogicStatusCategory> LogicStatusCategories { get; set; }

    //public virtual DbSet<LogicStatusCondition> LogicStatusCondictions { get; set; }

    //public virtual DbSet<ErrorCodeCategory> ErrorCodeCategories { get; set; }

    //public virtual DbSet<ErrorCodeMapping> ErrorCodeMappings { get; set; }

    public virtual DbSet<TaskRecordConfig> TaskRecordConfigs { get; set; }

    public virtual DbSet<TaskRecordContent> TaskRecordContents { get; set; }

    public virtual DbSet<TaskRecordDetail> TaskRecordDetails { get; set; }

    public virtual DbSet<Workorder> Workorders { get; set; }

    public virtual DbSet<WorkorderRecipeConfig> WorkorderRecipeConfigs { get; set; }

    //public virtual DbSet<WorkorderRecipeContent> WorkorderRecipeContents { get; set; }

    public virtual DbSet<RecipeItemBase> RecipeBases { get; set; }

    public virtual DbSet<WorkorderRecordConfig> WorkorderRecordConfigs { get; set; }

    public virtual DbSet<WorkorderRecordContent> WorkorderRecordContents { get; set; }

    public virtual DbSet<WorkorderRecordDetail> WorkorderRecordDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemDeta__3214EC27CD8B5278");

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
                .OnDelete(DeleteBehavior.ClientCascade)
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
            entity.HasKey(e => e.Id).HasName("PK__ItemReco__3214EC27CF6CF0E6");

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
            entity.HasKey(e => new { e.ItemId, e.RecordContentId }).HasName("PK__ItemReco__68A347184281468E");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.ItemRecordDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__ItemRecor__ItemI__6477ECF3");

            entity.HasOne(d => d.RecordContent).WithMany(p => p.ItemRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasConstraintName("FK__ItemRecor__Recor__656C112C");
        });

        //modelBuilder.Entity<MachineBase>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PK__Machine__3214EC274421D01E");
        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");
        //    entity.HasIndex(e => e.Name, "UQ__Machine__737584F66D3A72BE").IsUnique();
        //    entity.Property(e => e.Name).HasMaxLength(50);
        //    entity.Property(e => e.Ip)
        //        .HasMaxLength(50)
        //        .HasColumnName("IP");
        //    entity.Property(e => e.TagCategoryId).HasColumnName("TagCategoryID");
        //    entity.HasOne(d => d.TagCategory).WithMany(p => p.Machines)
        //        .HasForeignKey(d => d.TagCategoryId)
        //        .HasConstraintName("FK__Machine__TagCate__6477ECF3");

        //    entity.Property(e => e.UpdateDelay).HasColumnName("UpdateDelay");
        //});

        //modelBuilder.Entity<Machine>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PK__Machine__3214EC274421D01E");
        //    entity.UseTpcMappingStrategy();
        //    entity.ToTable("Machine");

        //    entity.HasIndex(e => e.Name, "UQ__Machine__737584F66D3A72BE").IsUnique();

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");
        //    entity.Property(e => e.Ip)
        //        .HasMaxLength(50)
        //        .HasColumnName("IP");

        //    entity.Property(e => e.Name).HasMaxLength(50);
        //    //entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
        //    entity.Property(e => e.TagCategoryId).HasColumnName("TagCategoryID");
        //    entity.Property(e => e.LogicStatusCategoryId).HasColumnName("LogicStatusCategoryID");

        //    //entity.HasOne(d => d.Process).WithMany(p => p.Machines)
        //    //    .HasForeignKey(d => d.ProcessId)
        //    //    .HasConstraintName("FK_Machine_Process");

        //    entity.HasOne(d => d.TagCategory).WithMany(p => p.Machines)
        //        .HasForeignKey(d => d.TagCategoryId)
        //        .HasConstraintName("FK__Machine__TagCate__6477ECF3");

        //    entity.HasOne(d => d.LogicStatusCategory).WithMany(p => p.Machines)
        //        .HasForeignKey(d => d.LogicStatusCategoryId);

        //    entity.HasOne(d => d.ErrorCodeCategory).WithMany(p => p.Machines)
        //        .HasForeignKey(d => d.ErrorCodeCategoryId);

        //    entity.Property(e => e.Enabled).HasColumnName("Enabled");
        //    entity.Property(e => e.UpdateDelay).HasColumnName("UpdateDelay");
        //    entity.Property(e => e.MaxRetryCount).HasColumnName("MaxRetryCount");
        //});

        modelBuilder.Entity<MachineStatusLog>(entity =>
        {

            entity.ToTable("MachineStatusLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MachineID).HasColumnName("MachineID");
            entity.Property(e => e.Status).HasColumnName("Status");
            entity.Property(e => e.LogTime).HasColumnName("LogTime");
        });

        modelBuilder.Entity<MapComponent>(entity =>
        {
            entity.ToTable("MapComponent");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
            entity.Property(e => e.MapId).HasColumnName("MapID");
            entity.Property(e => e.PositionX).HasColumnName("Position_x");
            entity.Property(e => e.PositionY).HasColumnName("Position_y");

            entity.HasOne(d => d.Map).WithMany(p => p.MapComponents)
                .HasForeignKey(d => d.MapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MapComponent");
        });

        modelBuilder.Entity<MapConfig>(entity =>
        {
            entity.ToTable("MapConfig");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Image).WithMany(p => p.MapConfigs)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MapImage");
        });

        modelBuilder.Entity<MapImage>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.DataByte).HasColumnType("image");
            entity.Property(e => e.DataType).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Process__3214EC277C713F4A");

            entity.ToTable("Process");

            entity.HasIndex(e => e.Name, "UQ__Process__737584F604F6A567").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ProcessMachineRelation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("ProcessMachineRelations");

            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.MachineId).HasColumnName("MachineID");
        });

        modelBuilder.Entity<ScriptConfig>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ScriptName).HasMaxLength(50);
            entity.Property(e => e.AutoCompile).HasColumnName("AutoCompile");
            entity.Property(e => e.AutoRun).HasColumnName("AutoRun");
            entity.Property(e => e.SuorceCode).HasColumnName("SourceCode").HasColumnType("VARCHAR(MAX)");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stations__3214EC2739A5326C");

            entity.HasIndex(e => e.Name, "UQ__Stations__737584F624E8C286").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");

            entity.HasOne(d => d.Process).WithMany(p => p.Stations)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Stations__Proces__6754599E");
        });

        modelBuilder.Entity<StationUirecord>(entity =>
        {
            entity.ToTable("StationUIRecord");

            entity.HasIndex(e => e.StationId, "IX_StationUIRecord");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ItemRecordId).HasColumnName("ItemRecordID");
            entity.Property(e => e.StationId).HasColumnName("StationID");

            entity.HasOne(d => d.ItemRecord).WithMany(p => p.StationUirecords)
                .HasForeignKey(d => d.ItemRecordId)
                .HasConstraintName("FK_StationUIRecord_ItemRecordContents");

            entity.HasOne(d => d.Station).WithMany(p => p.StationUirecords)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK_StationUIRecord_Stations");
        });

        //modelBuilder.Entity<TagCategory>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PK__TagCateg__3214EC272BE5477F");

        //    entity.ToTable("TagCategory");

        //    entity.HasIndex(e => e.Name, "UQ__TagCateg__737584F667A64B71").IsUnique();

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");
        //    entity.Property(e => e.Name).HasMaxLength(50);
        //});

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tag__3214EC271E77490D");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "UQ__Tag__737584F665FA3403").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Bool1).HasColumnName("Bool_1");
            entity.Property(e => e.Bool2).HasColumnName("Bool_2");
            entity.Property(e => e.Bool3).HasColumnName("Bool_3");
            entity.Property(e => e.Bool4).HasColumnName("Bool_4");
            entity.Property(e => e.Bool5).HasColumnName("Bool_5");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Int1).HasColumnName("Int_1");
            entity.Property(e => e.Int2).HasColumnName("Int_2");
            entity.Property(e => e.Int3).HasColumnName("Int_3");
            entity.Property(e => e.Int4).HasColumnName("Int_4");
            entity.Property(e => e.Int5).HasColumnName("Int_5");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.String1)
                .HasMaxLength(50)
                .HasColumnName("String_1");
            entity.Property(e => e.String2)
                .HasMaxLength(50)
                .HasColumnName("String_2");
            entity.Property(e => e.String3)
                .HasMaxLength(50)
                .HasColumnName("String_3");
            entity.Property(e => e.String4)
                .HasMaxLength(50)
                .HasColumnName("String_4");
            entity.Property(e => e.String5)
                .HasMaxLength(50)
                .HasColumnName("String_5");

            entity.HasOne(d => d.Category).WithMany(p => p.Tags)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientCascade);
                //.HasConstraintName("FK__Tag__CategoryID__68487DD7");
        });

        //modelBuilder.Entity<LogicStatusCategory>(entity =>
        //{
        //    entity.HasKey(e => e.Id);

        //    entity.ToTable("LogicStatusCategories");

        //    entity.HasIndex(e => e.Name).IsUnique();

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");

        //    entity.Property(e => e.Name).HasMaxLength(50);
        //});

        //modelBuilder.Entity<LogicStatusCondition>(entity =>
        //{
        //    entity.HasKey(e => e.Id);

        //    entity.ToTable("LogicStatusConditions");

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");
        //    entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        //    entity.Property(e => e.ConditionString)
        //        .HasMaxLength(50)
        //        .HasColumnName("CondictionString");

        //    entity.Property(e => e.Status).HasColumnName("Status");

        //    entity.HasOne(d => d.Category).WithMany(p => p.LogicStatusConditions)
        //        .HasForeignKey(d => d.CategoryId)
        //        .OnDelete(DeleteBehavior.ClientCascade);
        //});

        //modelBuilder.Entity<ErrorCodeCategory>(entity =>
        //{
        //    entity.HasKey(e => e.Id);

        //    entity.ToTable("ErrorCodeCategories");

        //    entity.HasIndex(e => e.Name).IsUnique();

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");

        //    entity.Property(e => e.Name).HasMaxLength(50);
        //});

        //modelBuilder.Entity<ErrorCodeMapping>(entity =>
        //{
        //    entity.HasKey(e => e.Id);

        //    entity.ToTable("ErrorCodeMappings");

        //    entity.Property(e => e.Id)
        //        .ValueGeneratedNever()
        //        .HasColumnName("ID");
        //    entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        //    entity.Property(e => e.ConditionString)
        //        .HasMaxLength(50)
        //        .HasColumnName("ConditionString");
        //    entity.Property(e => e.Description)
        //        .HasMaxLength(50)
        //        .HasColumnName("Description");
        //    entity.HasOne(d => d.Category).WithMany(p => p.ErrorCodeMappings)
        //        .HasForeignKey(d => d.CategoryId)
        //        .OnDelete(DeleteBehavior.ClientCascade);
        //});

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
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TaskDetai__ItemI__534D60F1");

            entity.HasOne(d => d.Station).WithMany(p => p.TaskDetails)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK__TaskDetai__Stati__6A30C649");
        });

        modelBuilder.Entity<TaskRecordConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskReco__3214EC275ECE0356");

            entity.HasIndex(e => e.TaskRecordsCategory, "UQ__TaskReco__C644E696EB97618B").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.TaskRecordsCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskRecordContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskReco__3214EC272D4C5C93");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecordName).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.TaskRecordContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__TaskRecor__Confi__04E4BC85");
        });

        modelBuilder.Entity<TaskRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.RecordContentId }).HasName("PK__TaskReco__66B48D22360FA6F1");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.RecordContent).WithMany(p => p.TaskRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskRecor__Recor__05D8E0BE");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskRecordDetails)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskRecor__TaskI__5070F446");
        });

        modelBuilder.Entity<Workorder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workerde__3214EC27B578A3E9");

            entity.HasIndex(e => new { e.WorkorderNo, e.Lot }, "Workerders_index_0").IsUnique();

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
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.TaskRecordCategoryId).HasColumnName("TaskRecordCategoryID");
            entity.Property(e => e.WorkorderNo).HasMaxLength(50);
            entity.Property(e => e.WorkorderRecordCategoryId).HasColumnName("WorkorderRecordCategoryID");

            entity.HasOne(d => d.ItemRecordsCategory).WithMany(p => p.Workorders)
                .HasForeignKey(d => d.ItemRecordsCategoryId)
                .HasConstraintName("FK__Workerder__ItemR__4AB81AF0");

            entity.HasOne(d => d.Process).WithMany(p => p.Workorders)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workerder__Proce__02FC7413");

            entity.HasOne(d => d.RecipeCategory).WithMany(p => p.Workorders)
                .HasForeignKey(d => d.RecipeCategoryId)
                .HasConstraintName("FK__Workerder__Recip__4316F928");

            entity.HasOne(d => d.TaskRecordCategory).WithMany(p => p.Workorders)
                .HasForeignKey(d => d.TaskRecordCategoryId)
                .HasConstraintName("FK__Workerder__TaskR__4E88ABD4");

            entity.HasOne(d => d.WorkorderRecordCategory).WithMany(p => p.Workorders)
                .HasForeignKey(d => d.WorkorderRecordCategoryId)
                .HasConstraintName("FK__Workerder__Worko__46E78A0C");
        });

        modelBuilder.Entity<WorkorderRecipeConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC271196F08D");

            entity.HasIndex(e => e.RecipeCategory, "UQ__Workorde__46E6E02C5E834A8B").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.RecipeCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<RecipeItemBase>(entity =>
        {
            entity.UseTpcMappingStrategy();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecipeItemName).HasColumnName("RecipeItemName");
            entity.Property(e => e.TriggerTiming).HasColumnName("TriggerTiming");
            entity.Property(e => e.DataType).HasColumnName("DataType");
            entity.Property(e => e.TargetTagCatId).HasColumnName("TargetTagCatID");
            entity.Property(e => e.TargetTagId).HasColumnName("TargetTagID");

            entity.HasOne(d => d.Config).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.ConfigId);
        });

        modelBuilder.Entity<StaticRecipe>(entity =>
        {
            entity.ToTable("StaticRecipes");
            entity.Property(e => e.ValueString).HasColumnName("ValueString");


        });

        modelBuilder.Entity<BuildInRecipe>(entity =>
        {
            entity.ToTable("BuildInRecipes");
            entity.Property(e => e.TargetProp).HasColumnName("TargetProp");
        });

        modelBuilder.Entity<CustomRecipe>(entity =>
        {
            entity.ToTable("CustomRecipes");
            entity.Property(e => e.TargetRecordCatID).HasColumnName("TargetRecordCatID");
            entity.Property(e => e.TargetRecordID).HasColumnName("TargetRecordID");
        });

        modelBuilder.Entity<WorkorderRecordConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC27588443F9");

            entity.HasIndex(e => e.WorkorderRecordCategory, "UQ__Workorde__95EF0114D7717B84").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.WorkorderRecordCategory).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkorderRecordContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workorde__3214EC27C5678B6F");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.RecordName).HasMaxLength(50);

            entity.HasOne(d => d.Config).WithMany(p => p.WorkorderRecordContents)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__Workorder__Confi__0B91BA14");
        });

        modelBuilder.Entity<WorkorderRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.WorkerderId, e.RecordContentId }).HasName("PK__Workorde__E4F8D8DC68390356");

            entity.Property(e => e.WorkerderId).HasColumnName("WorkerderID");
            entity.Property(e => e.RecordContentId).HasColumnName("RecordContentID");
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.RecordContent).WithMany(p => p.WorkorderRecordDetails)
                .HasForeignKey(d => d.RecordContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workorder__Recor__0C85DE4D");

            entity.HasOne(d => d.Workerder).WithMany(p => p.WorkorderRecordDetails)
                .HasForeignKey(d => d.WorkerderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workorder__Worke__48CFD27E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
