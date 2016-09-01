using EpocaEspecial.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.Code
{
    public class Enhancer
    {
        public Type t;
        private TypeBuilder newT;
        private List<Attribute> allAttributes = new List<Attribute>();
        public T Build<T>(params object[] args)
        {
            Type t = typeof(T);
            MethodBase[] mb = t.GetMethods();
            Type newType = CreateInstance(t);
            object o = Activator.CreateInstance(newType, args);
            return (T)o;
        }

        public Type CreateInstance(Type t, params object[] args)
        {
            string n = "newStock" + t.Name;
            string MOD_NAME = n;
            string DLL_NAME = n + ".dll";
            string ASM_NAME = n;
            string TYP_NAME = n;
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(ASM_NAME),
                AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder mb = ab.DefineDynamicModule(MOD_NAME, DLL_NAME);

            newT = mb.DefineType(TYP_NAME, TypeAttributes.Public, t);
            Type newType = BuildStructure(t);

            ab.Save(DLL_NAME);
            //Criar construtor, metodos,etc
            return newT.CreateType();

        }

        private Type BuildStructure(Type tb)
        {
            CreateConstructors(tb);
            CreateProperties(tb);
            CreateMethods(tb);
            return newT.CreateType();
        }

        private void CreateMethods(Type tb)
        {
            MethodInfo[] mi = tb.GetMethods();
            foreach (MethodInfo m in mi)
            {
                if (m.IsVirtual)
                {
                    if (m.GetCustomAttributes(typeof(IAttributes), false).Length > 0)
                    {
                        MethodAttributes methodFlags = MethodAttributes.Public | MethodAttributes.Virtual;
                        MethodBuilder setPropMthdBldr =
                            newT.DefineMethod(m.Name,
                              methodFlags,
                              m.ReturnType,  m.GetParameters().Select(x => x.ParameterType).ToArray() );
                       
                        Type[] i = new Type[m.GetParameters().Length];
                        int idx = 0;
                        ILGenerator il = setPropMthdBldr.GetILGenerator();
                        List<Attribute> list = m.GetCustomAttributes(typeof(Attribute)).ToList();
                        ParameterInfo[] p = m.GetParameters().ToArray();
                        Type[] type = new Type[p.Length];
                        for (int x = 0; x < p.Length; ++x)
                            type[x] = p[x].ParameterType;

                        CustomAttributeData[] attrDataArray = m.GetCustomAttributesData().ToArray();
                      
                        foreach (Attribute a in list)
                        {
                            Type[] attrsType = attrDataArray[idx].ConstructorArguments.Select(me => me.ArgumentType).ToArray();

                            var baseCtor = a.GetType().GetConstructor(attrDataArray[idx].ConstructorArguments.Select(me => me.ArgumentType).ToArray());


                            if (attrDataArray[idx].ConstructorArguments.Count > 0)
                            {
                                if (attrsType.First().IsArray)
                                {
                                    IReadOnlyCollection<CustomAttributeTypedArgument> val = (IReadOnlyCollection<CustomAttributeTypedArgument>)attrDataArray[idx].ConstructorArguments.First().Value; //count
                                    LocalBuilder array = il.DeclareLocal(attrsType.GetType());
                                    il.Emit(OpCodes.Ldc_I4, val.Count);
                                    il.Emit(OpCodes.Newarr, typeof(object));
                                    il.Emit(OpCodes.Stloc, array);
                                    int id = 0;
                                    foreach (var o in val)
                                    {
                                        il.Emit(OpCodes.Ldloc, array);
                                        il.Emit(OpCodes.Ldc_I4, id);
                                        CastTypes(o.Value,il);/*
                                        if (o.Value is double)
                                        {
                                            il.Emit(OpCodes.Ldc_R8, (double)o.Value);
                                        }
                                        if (o.Value is int)
                                        {
                                            il.Emit(OpCodes.Ldc_I4, (int)o.Value);
                                        }
                                        if (o.Value is string)
                                        {
                                            il.Emit(OpCodes.Ldstr, (string)o.Value);
                                        }
                                        if (o.Value.GetType().IsPrimitive)
                                        {
                                            il.Emit(OpCodes.Box, o.Value.GetType());
                                        }*/
                                        il.Emit(OpCodes.Stelem_I4);
                                        id++;
                                    }
                                  
                                    ConstructorInfo ctor = a.GetType().GetConstructor(attrsType);
                                 
                                    il.Emit(OpCodes.Ldarg_0);
                                    il.Emit(OpCodes.Ldloc, array);
                                 
                                    il.Emit(OpCodes.Call, baseCtor);

                                }
                                else
                                {
                                    il.Emit(OpCodes.Ldarg_0);
                                    var aux = attrDataArray[idx].ConstructorArguments.First().Value;

                                    CastTypes(aux,il);
                              
                                    //     var baseCtor = a.GetType().GetConstructor(attrDataArray[idx].ConstructorArguments.Select(m => m.ArgumentType).ToArray());

                                    il.Emit(OpCodes.Call, baseCtor);



                                    //il.Emit(OpCodes.Ldarg_0);
                                }
                            }
                            ++idx;
                            LocalBuilder paramValues = il.DeclareLocal(typeof(object[]));
                            il.Emit(OpCodes.Ldc_I4_S, type.Length);
                            il.Emit(OpCodes.Newarr, typeof(object));
                            il.Emit(OpCodes.Stloc, paramValues);
                            for (int y = 0; y < type.Length; ++y)
                            {
                                il.Emit(OpCodes.Ldloc, paramValues);
                                il.Emit(OpCodes.Ldc_I4, y);
                                il.Emit(OpCodes.Ldarg, y + 1);
                                if (type[y].IsPrimitive)
                                    il.Emit(OpCodes.Box, type[y]);
                                il.Emit(OpCodes.Stelem_I4);
                            }
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldloc, paramValues);
                            il.Emit(OpCodes.Call, a.GetType().GetMethod("Validator"));
                        }
                        //load args to call base
                        il.Emit(OpCodes.Ldarg_0);
                        for (int z = 0; z < type.Length; ++z)
                        {
                            il.Emit(OpCodes.Ldarg, z + 1);
                        }
                        il.Emit(OpCodes.Call, tb.GetMethod(m.Name));
                        il.Emit(OpCodes.Ret);
                    }


                }
                }
            }

        
        private void CreateConstructors(Type tb)
        {
            ConstructorInfo[] ci = tb.GetConstructors();

            foreach (ConstructorInfo c in ci)
            {
                ParameterInfo[] pi = c.GetParameters();
                List<Type> parameters = new List<Type>();
                foreach (ParameterInfo p in pi)
                {
                    parameters.Add(p.ParameterType);
                    FieldBuilder fb = newT.DefineField(p.Name, p.ParameterType, FieldAttributes.Private);

                }
                ConstructorBuilder ctor = newT.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters.ToArray());
                ILGenerator ctorIL = ctor.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                int idx = 1;
                while (parameters.ToArray().Length >= idx)
                {
                    ctorIL.Emit(OpCodes.Ldarg, idx);
                    idx++;
                }
                ctorIL.Emit(OpCodes.Call, tb.GetConstructor(parameters.ToArray()));
                ctorIL.Emit(OpCodes.Ret);
            }
        }

        private void CreateProperties(Type tb)
        {
            PropertyInfo[] pi = tb.GetProperties();
            List<Attribute> attrs = new List<Attribute>();

            foreach (PropertyInfo p in pi)
            {
                Attribute[] att = p.GetCustomAttributes().ToArray();
                
                MethodBuilder setPropMthdBldr =
                        newT.DefineMethod("set_" + p.Name,
                          MethodAttributes.Public |
                          MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                          null, new Type[] { p.PropertyType });
                CustomAttributeData[] attrDataArray = p.GetCustomAttributesData().ToArray();
                ILGenerator il = setPropMthdBldr.GetILGenerator();
                int idx = 0;
                foreach (Attribute a in att)
                {
                    Type [] attrsType = attrDataArray[idx].ConstructorArguments.Select(m => m.ArgumentType).ToArray();

                    var baseCtor = a.GetType().GetConstructor(attrDataArray[idx].ConstructorArguments.Select(m => m.ArgumentType).ToArray());
                    

                    if (attrDataArray[idx].ConstructorArguments.Count > 0)
                    {
                        if (attrsType.First().IsArray)
                        {
                            IReadOnlyCollection<CustomAttributeTypedArgument> val = (IReadOnlyCollection<CustomAttributeTypedArgument>)attrDataArray[idx].ConstructorArguments.First().Value; //count
                            LocalBuilder array = il.DeclareLocal(attrsType.GetType());
                            il.Emit(OpCodes.Ldc_I4, val.Count);
                            il.Emit(OpCodes.Newarr, typeof(object));
                            il.Emit(OpCodes.Stloc, array);        

                            int id = 0;
                            foreach (var o in val)
                            {
                                il.Emit(OpCodes.Ldloc, array);
                                il.Emit(OpCodes.Ldc_I4, id);
                                CastTypes(o.Value, il);
                                il.Emit(OpCodes.Stelem_I4);
                                id++;
                            }
         
                            ConstructorInfo ctor = a.GetType().GetConstructor(attrsType);
                      
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldloc, array);
                          
                            il.Emit(OpCodes.Call, baseCtor);
                      
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            var aux = attrDataArray[idx].ConstructorArguments.First().Value;


                            CastTypes(aux, il);
                            il.Emit(OpCodes.Call, baseCtor);
                        }
                    }
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1); // valor a comparar
                    if (p.PropertyType.IsPrimitive)
                    {
                        il.Emit(OpCodes.Box, p.PropertyType);
                    }
                    il.Emit(OpCodes.Call, a.GetType().GetMethod("Validator"));
                    ++idx;
                }
                
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, tb.GetProperty(p.Name).GetSetMethod());
                il.Emit(OpCodes.Ret);
            }


        }
        private void CastTypes(object aux, ILGenerator il)
        {

            if (aux is double)
            {
                il.Emit(OpCodes.Ldc_R8, (double)aux);
            }
            if (aux is string)
            {
                il.Emit(OpCodes.Ldstr, (string)aux);
            }
            if (aux is int)
            {
                il.Emit(OpCodes.Ldc_I4, (int)aux);
            }
            if (aux.GetType().IsPrimitive)
            {
                il.Emit(OpCodes.Box, aux.GetType());
            }
        }
    }
}
     