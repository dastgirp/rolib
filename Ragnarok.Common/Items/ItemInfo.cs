using System.Data.Common;
using System.Data;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ragnarok
{
    public class ItemInfo
    {
        private struct E
        {
            public string Name;
            public string Prop;
            public object Default;
            public long Max;

            public E(string name, string prop, object def, long max)
            {
                Name = name;
                Prop = prop;
                Default = def;
                Max = max;
            }
        }

        private static E[] Fields = new E[]
        {
            new E("id", "ID", 0, ushort.MaxValue),
            new E("name_english", "Name", "", 50),
            new E("name_japanese", "NameInternal", "", 50),
            new E("type", "Type", (ItemType)0, byte.MaxValue),
            new E("price_buy", "Price_Buy", null, 16777215),
            new E("price_sell", "Price_Sell", null, 16777215),
            new E("weight", "Weight", 0, ushort.MaxValue),
            new E("attack", "Attack", null, ushort.MaxValue),
            new E("defence", "Defence", null, byte.MaxValue),
            new E("range", "Range", null, byte.MaxValue),
            new E("slots", "Slots", null, byte.MaxValue),
            new E("equip_jobs", "EquipJobs", null, uint.MaxValue),
            new E("equip_upper", "EuipUpper", null, byte.MaxValue),
            new E("equip_genders", "EquipGenders", null, byte.MaxValue),
            new E("weapon_level", "WeaponLevel", null, byte.MaxValue),
            new E("equip_level", "EquipLevel", null, byte.MaxValue),
            new E("refinable", "Refinable", null, 1),
            new E("view", "View", null, ushort.MaxValue),
            new E("script", "Script", "", -1),
            new E("equip_script", "EquipScript", "", -1),
            new E("unequip_script", "UnequipScript", "", -1)
        };

        public bool Custom { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string NameInternal { get; set; }
        public ItemType Type { get; set; }
        public int? Price_Buy { get; set; }
        public int? Price_Sell { get; set; }
        public int Weight { get; set; }
        public int? Attack { get; set; }
        public int? Defence { get; set; }
        public int? Range { get; set; }
        public int? Slots { get; set; }
        public EquipJobs? EquipJobs { get; set; }
        public EquipUpper? EquipUpper { get; set; }
        public Sex? EquipGenders { get; set; }
        public EquipLocation? EquipLocations { get; set; }
        public int? WeaponLevel { get; set; }
        public int? EquipLevel { get; set; }
        public bool? Refinable { get; set; }
        public int? View { get; set; }
        public string Script { get; set; }
        public string EquipScript { get; set; }
        public string UnequipScript { get; set; }

        public ItemInfo()
        {

        }

        public ItemInfo(DbDataReader reader)
        {
            if (reader.IsClosed || !reader.HasRows)
                throw new Exception("Reader is closed or has no rows.");

            try
            {
                Type t = GetType();
                PropertyInfo pi;
                int o = -1;

                foreach (E fld in Fields)
                {
                    pi = t.GetProperty(fld.Prop);

                    try
                    {
                        o = reader.GetOrdinal(fld.Name);
                    }
                    catch (OutOfMemoryException)
                    {
                        o = -1;
                    }

                    pi.SetValue(this, (o < 0 || reader.IsDBNull(o)) ? fld.Default : reader[o], null);
                }
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while reading.");
            }
        }

        public ItemInfo(DbDataAdapter adapter)
        {
            DataSet set = new DataSet();
            adapter.Fill(set);

            //set.Tables[0].Rows[0]

            throw new NotImplementedException();
        }

        public static ItemInfo[] GetItems(DbDataReader reader)
        {
            List<ItemInfo> ret = new List<ItemInfo>();

            while (reader.Read())
                ret.Add(new ItemInfo(reader));

            return ret.ToArray();
        }

        public static ItemInfo[] GetItems(DbDataAdapter adapter)
        {
            List<ItemInfo> ret = new List<ItemInfo>();

            throw new NotImplementedException();
        }
    }
}
