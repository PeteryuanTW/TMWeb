﻿using CommonLibrary.MachinePKG;
using DevExpress.XtraPrinting;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Serilog;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace TMWeb.EFModels
{
    public partial class RecipeItem
    {
        public RecipeItem() { }
        public RecipeItem(Guid configId)
        {
            Id = Guid.NewGuid();
            ConfigId = configId;
        }

        public RecipeItem Copy(Guid catId)
        {
            return new RecipeItem()
            {
                Id = Guid.NewGuid(),
                ConfigId = catId,
                RecipeItemName = this.RecipeItemName,
                TriggerTiming = this.TriggerTiming,
                TargetTagCatId = this.TargetTagCatId,
                TargetTagId = this.TargetTagId,
                DataType = this.DataType,
                ValueExpString = this.ValueExpString,
            };
        }

        public bool hasTargetTag => TargetTagCatId != null && TargetTagId != null;
        public bool hasDatatype => DataType != null;
        public bool hasTarget => hasTargetTag && hasDatatype;
        public async Task<Tuple<bool, string, Object?>> GetValue(Workorder wo)
        {
            try
            {
                string expStrClone = ValueExpString;
                var res = Regex.Matches(expStrClone, @"\#(.*?)\#");
                var type = MachineTypeEnumHelper.GetTypeByCode(DataType);
                if (res is not null)
                {
                    var dict = wo.GetVariableList();
                    foreach (Match match in res)
                    {
                        var keyWithBraces = match.Value;
                        string key = keyWithBraces.Substring(1, keyWithBraces.Length - 2);
                        if (dict.ContainsKey(key))
                        {
                            expStrClone = expStrClone.Replace(keyWithBraces, dict[key].ToString());
                        }
                        else
                        {
                            return new(false, RecipeItemName , null);
                        }
                    }
                }
                if (type == typeof(bool))
                {
                    var boolRes = Convert.ToBoolean(ValueExpString);
                    return new(true, RecipeItemName, boolRes);
                }
                else if (type == typeof(ushort))
                {
                    var ushortRes = Convert.ToUInt16(ValueExpString);
                    return new(true, RecipeItemName, ushortRes);
                }
                else if (type == typeof(string))
                {
                    return new(true, RecipeItemName, expStrClone);
                }
                else
                {
                    return new(false, RecipeItemName, "type error");
                }
            }
            catch (Exception e)
            {
                return new(false, RecipeItemName, e.Message);
            }

        }
        public async Task<Tuple<bool, string, string>> GetValueString(Workorder wo)
        {
            var objRes = await GetValue(wo);
            //Log.Information($"RecipeItem {RecipeItemName} get value result: {objRes.Item1}, {objRes.Item2}, {objRes.Item3?.ToString()}");
            return objRes.Item1 ? new(objRes.Item1, objRes.Item2, objRes.Item3.ToString()) : new(objRes.Item1, objRes.Item2, null);
        }
    }
}
