namespace PCPartsWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddressAndPhoneToOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "PhoneNumber", c => c.String());
            AddColumn("dbo.Orders", "Address", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "Address");
            DropColumn("dbo.Orders", "PhoneNumber");
        }
    }
}
