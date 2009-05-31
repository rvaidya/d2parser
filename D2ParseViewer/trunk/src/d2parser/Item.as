package d2parser
{
	import flash.events.IEventDispatcher;
	
	public class Item
	{
		public var Id:int;
		public var Index:int;
		public var Realm:String;
		public var Character:String;
		public var OwnerType:String;
		public var Name:String;
		public var Color:String;
		public var TextColor:int;
		public var Level:int;
		public var BaseItem:String;
		public var Flags:String;
		public var Quality:String;
		public var Stats:String;
		public var Mods:String;
		public var Runeword:String;
		public var RunewordId:int;
		public var RunewordParam:String;
		public var Prefix:String;
		public var PrefixVar:int;
		public var Suffix:String;
		public var SuffixVar:int;
		public var Image:String;
		public var Location:String;
		public var Container:String;
		public var X:int;
		public var Y:int;
		public var SizeX:int;
		public var SizeY:int;
		public function Item()
		{
			Id = -1;
			Index = 0;
			Name = "";
			Realm = "";
			Character = "";
			OwnerType = "";
			Color = "";
			TextColor = 0;
			Level = 0;
			BaseItem = "";
			Flags = "";
			Quality = "";
			Stats = "";
			Mods = "";
			Runeword = "";
			RunewordId = -1;
			RunewordParam = "";
			Prefix = "";
			PrefixVar = -1;
			Suffix = "";
			SuffixVar = -1;
			Image = "";
			Location = "";
			Container = "";
			X = -1;
			Y = -1;
			SizeX = 0;
			SizeY = 0;
		}

	}
}