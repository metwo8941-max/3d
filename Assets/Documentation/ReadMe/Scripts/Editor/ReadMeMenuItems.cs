using RMC.Core.ReadMe;
using UnityEditor;

namespace RMC.d.ReadMe
{
	public static class ReadMeMenuItems
	{
		//  Fields ----------------------------------------
		public const string PathMenuItemWindowCompanyProject = "Window/" + CompanyName + "/" + ProjectName;
		public const string CompanyName = "RMC";
		public const string ProjectName = "d";
		public const int PriorityMenuItem_Examples = -1000;
        
		//  Fields ----------------------------------------
		
		[MenuItem( PathMenuItemWindowCompanyProject + "/" + "Open ReadMe", 
			false,
						PriorityMenuItem_Examples)]
		public static void SelectReadmes()
		{
			ReadMeHelper.SelectReadmes();
		}
	}
}
