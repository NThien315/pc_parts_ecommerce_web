namespace PCPartsWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixBrandId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Brands", "Origin", c => c.String(maxLength: 100));
            AlterColumn("dbo.Brands", "BrandName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Brands", "BrandName", c => c.String());
            DropColumn("dbo.Brands", "Origin");
        }
    }
}
