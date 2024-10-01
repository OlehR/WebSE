using System.Text.Json.Serialization;

namespace WebSE.RecieptModels
{
    public class WareHouse
    {
        public int Code {  get; set; }
        public string Name { get; set; }
        public string Adres {  get; set; }
        public string IdRref { get; set; }
        [JsonIgnore]
        public byte[] _IdRref { get; set; }
        public string Square { get; set; }
        public string GLN { get; set; }
        public int TypeWarehouse { get; set; }
        public int NumberWarehouse { get; set; }
        public string location { get; set; }
        public int CodeCompany { get; set; }
        public int CodeTM { get; set; }
        public string NameTM { get; set; }
        public string CodeShop { get; set; }
        public string GPS { get; set; }
        public int ID_SU { get; set; }
        public string Schedule { get; set; }
        [JsonIgnore]
        public byte[] SubDivisionRref { get; set; }
        public int Code_tw { get; set; }
        public int CodeWarehouse2 { get; set; }
        public int CodeWarehouseadd { get; set; }
        public int FormatWarehouse {  get; set; }
        public int CodeWarehouseDefective { get; set; }
        public int IsEditOS { get; set; }
        public string DNSPrefix { get; set; }

        public WareHouse()
        {
        }

        public WareHouse(int code, string name, string adres, string idRref, byte[] _idRref, string square, string gLN, int typeWarehouse, int numberWarehouse, string location, int codeCompany, int codeTM, string nameTM, string codeShop, string gPS, int iD_SU, string schedule, byte[] subDivisionRref, int code_tw, int codeWarehouse2, int codeWarehouseadd, int formatWarehouse, int codeWarehouseDefective, int isEditOS, string dNSPrefix)
        {
            Code = code;
            Name = name;
            Adres = adres;
            this.IdRref = idRref;
            this._IdRref = _idRref;
            Square = square;
            GLN = gLN;
            TypeWarehouse = typeWarehouse;
            NumberWarehouse = numberWarehouse;
            this.location = location;
            CodeCompany = codeCompany;
            CodeTM = codeTM;
            NameTM = nameTM;
            CodeShop = codeShop;
            GPS = gPS;
            ID_SU = iD_SU;
            Schedule = schedule;
            SubDivisionRref = subDivisionRref;
            Code_tw = code_tw;
            CodeWarehouse2 = codeWarehouse2;
            CodeWarehouseadd = codeWarehouseadd;
            FormatWarehouse = formatWarehouse;
            CodeWarehouseDefective = codeWarehouseDefective;
            IsEditOS = isEditOS;
            DNSPrefix = dNSPrefix;
        }
    }
}
