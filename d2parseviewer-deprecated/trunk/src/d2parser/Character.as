package d2parser
{
	import mx.collections.ArrayCollection;
	
	public class Character
	{
		public var Name:String;
		public var Class:String;
		public var Account:String;
		public var Realm:String;
		public var UsedSpace:int;
		public var Level:int;
		public var Inventory:ArrayCollection;
		public function Character()
		{
			Name= "";
			Account = "";
			Class = "";
			Realm = "";
			Inventory = new ArrayCollection;
			Level = 0;
			UsedSpace = 0;
		}

	}
}