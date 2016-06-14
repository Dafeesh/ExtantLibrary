using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Extant;
using Extant.Extensions;

using ProtoBuf;
using ProtoBuf.Meta;

namespace Extant.Net.Contract
{
    static internal class NetPacketResolver
    {
        private static Dictionary<Type, Dictionary<int, Type>> _resolveBank = new Dictionary<Type, Dictionary<int, Type>>();

        internal static bool TryRegisterContractsInContainer(Type containerClass, out string errorDump)
        {
            try
            {
                foreach (var contract in FindAllContractsInType(containerClass))
                {
                    RegisterTypeResolution(contract);
                    Console.WriteLine("Registered contract: " + contract.ContractType.ToString());
                    foreach (var mem in contract.Members)
                    {
                        Console.WriteLine("\t[" + mem.Attribute.Order + "] " + mem.Info.Name + " : " + (
                            (mem.Info.MemberType == MemberTypes.Field) ? ((FieldInfo)mem.Info).FieldType.ToString() : (
                            (mem.Info.MemberType == MemberTypes.Property) ? ((PropertyInfo)mem.Info).PropertyType.ToString() : "UNKNOWN")));
                    }
                }
                errorDump = null;
                return true;
            }
            catch (InvalidContractException e)
            {
                errorDump = e.ToString();
                return false;
            }
        }

        private static IEnumerable<ContractDef> FindAllContractsInType(Type containerType)
        {
            NetContractAttribute contractAttribute;
            NetContractMemberAttribute memberAttribute;
            List<MemberInfo> allMembers = new List<MemberInfo>();
            List<ContractMember> contractMembers = new List<ContractMember>();

            foreach (Type contractType in containerType.GetNestedTypes(BindingFlags.Public))
            {
                if ((contractAttribute = contractType.GetCustomAttributes(typeof(NetContractAttribute), true).FirstOrDefault() as NetContractAttribute) != null)
                {
                    if (contractType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                    null, Type.EmptyTypes, null) == null)
                        throw new InvalidContractException("Contract \"" + contractType.ToString() + "\" does not have a parameterless constructor! " +
                                "Add a parameterless constructor (can be private).");

                    allMembers.Clear();
                    allMembers.AddRange(contractType.GetProperties());
                    allMembers.AddRange(contractType.GetFields());

                    contractMembers.Clear();
                    foreach (var member in allMembers)
                        if ((memberAttribute = member.GetCustomAttributes(typeof(NetContractMemberAttribute), true).FirstOrDefault() as NetContractMemberAttribute) != null)
                            contractMembers.Add(new ContractMember(memberAttribute, member));

                    if (contractMembers.Count > 0)
                        yield return new ContractDef(containerType, contractType, contractAttribute, contractMembers.ToArray());
                    else
                        throw new InvalidContractException("Failed to find any members in contract: " + contractType.ToString());

                }
            }
        }

        private static void RegisterTypeResolution(ContractDef def)
        {
            var groupResolve = _resolveBank.GetOrAddNew(def.GroupType, () => new Dictionary<int, Type>());

            if (groupResolve.ContainsKey(def.ContractAttribute.ID))
                throw new ArgumentException("\"" + def.ContractType.ToString() + "\" cannot be assigned to ID[" + def.ContractAttribute.ID + "] because " +
                    "it is currently registered to \"" + groupResolve[def.ContractAttribute.ID].ToString() + "\"");

            var typeMeta = RuntimeTypeModel.Default.Add(def.ContractType, false);
            foreach (var member in def.Members)
                typeMeta.AddField(member.Attribute.Order, member.Info.Name);

            groupResolve.Add(def.ContractAttribute.ID, def.ContractType);
        }

        internal static Type Resolve(Type group, int id)
        {
            try
            {
                return _resolveBank[group][id];
            }
            catch (Exception e)
            {
                throw new TypeLoadException(group + "[" + id + "] has not been registered to be resolved.", e);
            }
        }

        private class ContractDef
        {
            public Type GroupType { get; private set; }

            public Type ContractType { get; private set; }
            public NetContractAttribute ContractAttribute { get; private set; }

            public ContractMember[] Members { get; private set; }

            public ContractDef(
                Type group,
                Type contract,
                NetContractAttribute contractAttr,
                ContractMember[] members)
            {
                this.GroupType = group;
                this.ContractType = contract;
                this.ContractAttribute = contractAttr;
                this.Members = members;
            }
        }

        private class ContractMember
        {
            public NetContractMemberAttribute Attribute { get; private set; }
            public MemberInfo Info { get; private set; }

            public ContractMember(NetContractMemberAttribute attr, MemberInfo info)
            {
                this.Attribute = attr;
                this.Info = info;
            }
        }
    }
}
