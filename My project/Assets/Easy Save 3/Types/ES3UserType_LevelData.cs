using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isChanged", "disenableObjects", "disenableEnemy")]
	public class ES3UserType_LevelData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_LevelData() : base(typeof(LevelData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (LevelData)obj;
			
			writer.WriteProperty("isChanged", instance.isChanged, ES3Type_bool.Instance);
			writer.WriteProperty("disenableObjects", instance.disenableObjects, ES3Type_boolArray.Instance);
			writer.WriteProperty("disenableEnemy", instance.disenableEnemy, ES3Type_boolArray.Instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (LevelData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "isChanged":
						instance.isChanged = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "disenableObjects":
						instance.disenableObjects = reader.Read<System.Boolean[]>(ES3Type_boolArray.Instance);
						break;
					case "disenableEnemy":
						instance.disenableEnemy = reader.Read<System.Boolean[]>(ES3Type_boolArray.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_LevelDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_LevelDataArray() : base(typeof(LevelData[]), ES3UserType_LevelData.Instance)
		{
			Instance = this;
		}
	}
}