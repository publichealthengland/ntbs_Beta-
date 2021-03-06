﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ntbs_service.Migrations
{
    public partial class PopulateTBService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TBService",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "TBS0001", "Abingdon Community Hospital" },
                    { "TBS0168", "Queen Alexandra Hospital" },
                    { "TBS0169", "Queen Elizabeth Hospital" },
                    { "TBS0170", "Queen Elizabeth Hospital, King's Lynn" },
                    { "TBS0171", "Queen Mary'S Hospital [London]" },
                    { "TBS0172", "Queen Mary'S Hospital [Sidcup]" },
                    { "TBS0173", "Queen Victoria Hospital [East Grinstead]" },
                    { "TBS0174", "Radcliffe Infirmary" },
                    { "TBS0175", "Royal Devon & Exeter" },
                    { "TBS0176", "RDASH NHS Foundation Trust" },
                    { "TBS0177", "Royal Berkshire Hospital" },
                    { "TBS0178", "Royal Bolton Hospital" },
                    { "TBS0179", "Royal Brompton" },
                    { "TBS0180", "Royal Buckinghamshire Hospital" },
                    { "TBS0181", "Royal Derby Hospital" },
                    { "TBS0182", "Royal Free London TB Service" },
                    { "TBS0183", "Royal Hampshire County Hospital" },
                    { "TBS0184", "Royal Hospital Haslar" },
                    { "TBS0185", "Royal London Hospital" },
                    { "TBS0186", "Royal Marsden Chelsea" },
                    { "TBS0187", "Royal National Orthopaedic Hospital (Stanmore)" },
                    { "TBS0188", "Royal South Hants Hospital" },
                    { "TBS0189", "Royal Surrey County Hospital" },
                    { "TBS0190", "Royal Sussex County Hospital" },
                    { "TBS0191", "Royal Victoria Hospital [Folkestone]" },
                    { "TBS0192", "Runneymede Hospital" },
                    { "TBS0193", "Salford Royal" },
                    { "TBS0194", "Sandwell" },
                    { "TBS0195", "Sheffield Teaching Hospitals NHS Foundation Trust" },
                    { "TBS0196", "Shropshire & Telford" },
                    { "TBS0167", "Priory Hospital" },
                    { "TBS0197", "Sloane Hospital" },
                    { "TBS0166", "Princess Royal Hospital [West Sussex]" },
                    { "TBS0164", "Preston Hall Hospital" },
                    { "TBS0135", "North Hampshire Hospital" },
                    { "TBS0136", "North Middlesex Hospital" },
                    { "TBS0137", "North of Tyne TB Service" },
                    { "TBS0138", "North Tees and Hartlepool" },
                    { "TBS0139", "North West Anglia Foundation Trust" },
                    { "TBS0140", "Northamptonshire" },
                    { "TBS0141", "Northern Lincolnshire & Goole NHS Foundation Trust" },
                    { "TBS0142", "Nottingham" },
                    { "TBS0143", "Nuffield Health Brentwood Hospital" },
                    { "TBS0144", "Nuffield Health Cambridge Hospital" },
                    { "TBS0145", "Nuffield Health Chichester Hospital" },
                    { "TBS0146", "Nuffield Health Guildford Hospital" },
                    { "TBS0147", "Nuffield Health Haywards Heath Hospital" },
                    { "TBS0148", "Nuffield Health Leeds Hospital" },
                    { "TBS0149", "Nuffield Health Tunbridge Wells Hospital" },
                    { "TBS0150", "Nuffield Health Woking Hospital" },
                    { "TBS0151", "Nuffield Health York Hospital" },
                    { "TBS0152", "Oldchurch Hospital" },
                    { "TBS0153", "Open Door (Health)" },
                    { "TBS0154", "Ormskirk & District General Hospital" },
                    { "TBS0155", "Orpington Hospital" },
                    { "TBS0156", "Orsett Hospital" },
                    { "TBS0157", "Oxford University Hospitals NHS Foundation Trust" },
                    { "TBS0158", "Papworth Hospital" },
                    { "TBS0159", "Parkside Hospital" },
                    { "TBS0160", "Pennine Acute Hospitals NHS Trust" },
                    { "TBS0161", "Pinehill Hospital" },
                    { "TBS0162", "Pontefract General Infirmary" },
                    { "TBS0163", "Portland Hospital" },
                    { "TBS0165", "Princess Alexandra Hospital" },
                    { "TBS0198", "Somerfield Hospital" },
                    { "TBS0199", "Somerset" },
                    { "TBS0200", "South Tees NHS TB Trust TB Team" },
                    { "TBS0235", "Sunderland" },
                    { "TBS0236", "Swindon" },
                    { "TBS0237", "Swindon & Wiltshire" },
                    { "TBS0238", "Tameside General Hospital" },
                    { "TBS0239", "TB Service NCL - South Hub" },
                    { "TBS0240", "The London Clinic" },
                    { "TBS0241", "The Rotherham NHS Foundation Trust" },
                    { "TBS0242", "Tickhill Road Hospital" },
                    { "TBS0243", "Torbay" },
                    { "TBS0244", "Townlands Hospital" },
                    { "TBS0245", "Tunbridge Wells Hospital [Pembury]" },
                    { "TBS0246", "University Hospital Lewisham" },
                    { "TBS0247", "Upton Hospital [Slough]" },
                    { "TBS0248", "Victoria Hospital [Romford]" },
                    { "TBS0249", "Walsall" },
                    { "TBS0250", "Warders Medical Centre" },
                    { "TBS0251", "Warrington Hospital" },
                    { "TBS0252", "Water Eaton Health Centre" },
                    { "TBS0253", "West Hertfordshire Hospitals" },
                    { "TBS0254", "West Middlesex University Hospital" },
                    { "TBS0255", "West Park Hospital [Epsom]" },
                    { "TBS0256", "West Suffolk Hospital" },
                    { "TBS0257", "Western Community Hospital" },
                    { "TBS0258", "Weston" },
                    { "TBS0259", "Wexham Park/King Edward VII Hospitals" },
                    { "TBS0260", "Whipps Cross University Hospital" },
                    { "TBS0261", "Whiston Hospital" },
                    { "TBS0262", "Wolverhampton" },
                    { "TBS0263", "Worcestershire" },
                    { "TBS0234", "Stoke Mandeveille/Wycombe Hospitals" },
                    { "TBS0233", "Stepping Hill Hospital" },
                    { "TBS0232", "Staffordshire & Stoke-on-Trent" },
                    { "TBS0231", "St. Margaret's Hospital" },
                    { "TBS0201", "South Tyneside- Specialist Health Visitor TB and Migrant Health, based at Low Fell Clinic Gateshead" },
                    { "TBS0202", "South West Yorkshire Partnership NHS Foundation Trust (Barnsley)" },
                    { "TBS0203", "South West Yorkshire Partnership NHS Foundation Trust (Wakefield)" },
                    { "TBS0204", "Southampton General Hospital" },
                    { "TBS0205", "Southend Hospital" },
                    { "TBS0206", "Southport & Formby District General Hospital" },
                    { "TBS0207", "Spire Alexandra Hospital" },
                    { "TBS0208", "Spire Harpenden Hospital" },
                    { "TBS0209", "Spire Hartswood Hospital" },
                    { "TBS0210", "Spire Lea Cambridge Hospital" },
                    { "TBS0211", "Spire Longlands Consulting Rooms" },
                    { "TBS0212", "Spire St Saviour's Hospital" },
                    { "TBS0213", "Spire Tunbridge Wells Hospital" },
                    { "TBS0214", "Spire Wellesley Hospital" },
                    { "TBS0134", "North Downs Hospital" },
                    { "TBS0215", "St Albans City Hospital" },
                    { "TBS0217", "St Anthonys Hospital" },
                    { "TBS0218", "St Bartholomew's Hospital [London]" },
                    { "TBS0219", "St Bartholomews Hospital [Rochester]" },
                    { "TBS0220", "St George's Hospital" },
                    { "TBS0221", "St George's Hospital [Stafford]" },
                    { "TBS0222", "St Helens Hospital [Merseyside]" },
                    { "TBS0223", "St James Hospital [Southsea]" },
                    { "TBS0224", "St John & St Elizabeth Hospital" },
                    { "TBS0225", "St Mary's Hospital (Isle of Wight)" },
                    { "TBS0226", "St Mary's Hospital [Portsmouth]" },
                    { "TBS0227", "St Michael's Hospital [Braintree]" },
                    { "TBS0228", "St Pancras Hospital" },
                    { "TBS0229", "St Richard's Hospital" },
                    { "TBS0230", "St Thomas' Hospital" },
                    { "TBS0216", "St Anns Hospital [London]" },
                    { "TBS0264", "Worthing Hospital" },
                    { "TBS0133", "North Devon" },
                    { "TBS0131", "Queen's Hospital [Croydon]" },
                    { "TBS0035", "Cavell Hospital" },
                    { "TBS0036", "Central & North Lancashire (LTHTR/LCFT)" },
                    { "TBS0037", "Charter Medical Centre" },
                    { "TBS0038", "Chase Farm Hospital" },
                    { "TBS0039", "Chaucer Hospital" },
                    { "TBS0040", "CHCP (City Health Care Partnership)" },
                    { "TBS0041", "Chelsea & Westminster" },
                    { "TBS0042", "Chelsfield Park Hospital" },
                    { "TBS0043", "Cheshire East" },
                    { "TBS0044", "Chesterfield Royal Hospital" },
                    { "TBS0045", "Chiltern Hospital" },
                    { "TBS0046", "Church Hill House Hospital" },
                    { "TBS0047", "Clacton And District Hospital" },
                    { "TBS0048", "Clementine Churchill Hospital" },
                    { "TBS0049", "Colchester Hospital" },
                    { "TBS0050", "Conquest Hospital" },
                    { "TBS0051", "Cornwall" },
                    { "TBS0052", "Countess of Chester Hospital" },
                    { "TBS0053", "County Durham and Darlington NHS Foundation Trust TB Nursing Service" },
                    { "TBS0054", "Coventry & Warwickshire" },
                    { "TBS0055", "Crawley Hospital" },
                    { "TBS0056", "Cumbria: Morecambe Bay Trust" },
                    { "TBS0057", "Cumbria: North Cumbria UHT" },
                    { "TBS0058", "Darent Valley Hospital" },
                    { "TBS0059", "Doncaster & Bassetlaw" },
                    { "TBS0060", "Dorset" },
                    { "TBS0061", "Duchess of Kent Hospital" },
                    { "TBS0062", "Dudley" },
                    { "TBS0063", "East & North Hertfordshire Hospitals" },
                    { "TBS0034", "Calderdale & Huddersfield NHS FT" },
                    { "TBS0064", "East Dorset" },
                    { "TBS0033", "Burton" },
                    { "TBS0031", "Buckingham Hospital" },
                    { "TBS0002", "Addenbrooke's Hospital" },
                    { "TBS0003", "Airedale NHS Foundation Trust" },
                    { "TBS0004", "Amersham Hospital" },
                    { "TBS0005", "Andover War Memorial Hospital" },
                    { "TBS0006", "Arrowe Park Hospital" },
                    { "TBS0007", "Ashford & St Peter's Hospital (Chertsey)" },
                    { "TBS0008", "Ashford Hospital" },
                    { "TBS0009", "Barnet Hospital" },
                    { "TBS0010", "Basildon & Thurrock University Hospitals" },
                    { "TBS0011", "Baskingstoke and North Hampshire Hospital" },
                    { "TBS0012", "Bath" },
                    { "TBS0013", "Battle Hospital" },
                    { "TBS0014", "Bedford Hospital" },
                    { "TBS0015", "Benenden Hospital" },
                    { "TBS0016", "Berkshire Independent Hospital" },
                    { "TBS0017", "Beverley Westwood Hospital" },
                    { "TBS0018", "BHR University Hospitals NHS Trust" },
                    { "TBS0019", "Birmingham & Solihull" },
                    { "TBS0020", "Blackheath Hospital" },
                    { "TBS0021", "Bognor Regis War Memorial Hospital" },
                    { "TBS0022", "Bradford Royal Infirmary" },
                    { "TBS0023", "Bradford Teaching Hospitals NHS Foundation Trust" },
                    { "TBS0024", "Brentwood Community Hospital" },
                    { "TBS0025", "Bridgewater Community Healthcare" },
                    { "TBS0026", "Bridlington & District Hospital" },
                    { "TBS0027", "Brighton General Hospital" },
                    { "TBS0028", "Bristol" },
                    { "TBS0029", "Bromley TB Service" },
                    { "TBS0030", "Broomfield Hospital" },
                    { "TBS0032", "Buckland Hospital" },
                    { "TBS0065", "East Lancashire Health Trust" },
                    { "TBS0066", "East Surrey Hospital" },
                    { "TBS0067", "Eastbourne District General Hospital" },
                    { "TBS0102", "King'S College Hospital (Dulwich)" },
                    { "TBS0103", "Kings Oak Hospital" },
                    { "TBS0104", "Kings Park Hospital" },
                    { "TBS0105", "Kingston Hospital" },
                    { "TBS0106", "LCHC (Leeds Community Healthcare NHS Trust)" },
                    { "TBS0107", "Leicester Royal Infirmary" },
                    { "TBS0108", "Lincolnshire" },
                    { "TBS0109", "Liverpool" },
                    { "TBS0110", "Locala Community Partnerships (Greater Huddersfield)" },
                    { "TBS0111", "Locala Community Partnerships (North Kirklees)" },
                    { "TBS0112", "London Independent Hospital" },
                    { "TBS0113", "London NW Healthcare Central Middlesex" },
                    { "TBS0114", "London NW Healthcare Ealing" },
                    { "TBS0115", "London NW Healthcare Northwick Park" },
                    { "TBS0116", "Luton & Dunstable Hospital" },
                    { "TBS0117", "Lymington Hospital (Peripheral Clinic)" },
                    { "TBS0118", "Maidstone District General Hospital" },
                    { "TBS0119", "Manchester University NHS Foundation Trust (MFT)" },
                    { "TBS0120", "Mayday University Hospital" },
                    { "TBS0121", "Medway Maritime Hospital" },
                    { "TBS0122", "Memorial Hospital" },
                    { "TBS0123", "Mid Notts" },
                    { "TBS0124", "Mile End Hospital" },
                    { "TBS0125", "Milton Keynes University Hospital" },
                    { "TBS0126", "Mount Alvernia Hospital" },
                    { "TBS0127", "Mount Vernon Hospital" },
                    { "TBS0128", "National Hospital for Neurology & Neurosciences - Queen Square" },
                    { "TBS0129", "Nelson Hospital" },
                    { "TBS0130", "Newham Chest Clinic" },
                    { "TBS0101", "Kings College Hospital" },
                    { "TBS0100", "Kent Community Health NHS Foundation Trust" },
                    { "TBS0099", "Kent & Sussex Hospital" },
                    { "TBS0098", "Joyce Green Hospital" },
                    { "TBS0068", "Epsom and St Helier NHS Trust" },
                    { "TBS0069", "Erith & District Hospital" },
                    { "TBS0070", "Esperance Hospital" },
                    { "TBS0071", "Essex County Hospital" },
                    { "TBS0072", "Fawkham Manor Hospital" },
                    { "TBS0073", "Fishermead Medical Centre" },
                    { "TBS0074", "Fitzwilliam Hospital" },
                    { "TBS0075", "Frimley Park Hospital" },
                    { "TBS0076", "Gateshead- Specialist Health Visitor TB and Migrant Health, based at Low Fell Clinic Gateshead" },
                    { "TBS0077", "Gloucestershire" },
                    { "TBS0078", "Goole District Hospital" },
                    { "TBS0079", "Gravesend and North Kent Hospital" },
                    { "TBS0080", "Great Ormond Street Hospital Central London Site" },
                    { "TBS0081", "Guy's Hospital" },
                    { "TBS0132", "Norfolk & Norwich University Hospital" },
                    { "TBS0082", "Halton Hospital" },
                    { "TBS0084", "Harefield Hospital" },
                    { "TBS0085", "Harold Wood Hospital" },
                    { "TBS0086", "Harrogate and District NHS Foundation Trust" },
                    { "TBS0087", "Heart Hospital" },
                    { "TBS0088", "Heatherwood Hospital" },
                    { "TBS0089", "Herefordshire" },
                    { "TBS0090", "Hillingdon Hospital" },
                    { "TBS0091", "Holly House Hospital" },
                    { "TBS0092", "Homerton" },
                    { "TBS0093", "Horsham Hospital" },
                    { "TBS0094", "Hospital for Tropical Diseases" },
                    { "TBS0095", "Imperial College Healthcare" },
                    { "TBS0096", "Ipswich Hospital" },
                    { "TBS0097", "James Paget Hospital" },
                    { "TBS0083", "Hampshire Clinic" },
                    { "TBS0265", "Yeovil" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0001");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0002");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0003");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0004");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0005");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0006");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0007");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0008");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0009");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0010");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0011");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0012");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0013");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0014");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0015");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0016");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0017");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0018");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0019");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0020");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0021");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0022");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0023");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0024");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0025");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0026");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0027");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0028");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0029");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0030");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0031");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0032");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0033");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0034");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0035");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0036");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0037");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0038");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0039");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0040");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0041");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0042");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0043");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0044");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0045");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0046");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0047");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0048");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0049");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0050");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0051");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0052");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0053");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0054");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0055");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0056");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0057");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0058");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0059");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0060");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0061");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0062");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0063");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0064");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0065");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0066");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0067");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0068");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0069");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0070");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0071");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0072");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0073");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0074");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0075");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0076");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0077");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0078");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0079");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0080");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0081");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0082");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0083");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0084");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0085");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0086");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0087");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0088");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0089");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0090");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0091");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0092");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0093");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0094");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0095");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0096");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0097");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0098");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0099");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0100");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0101");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0102");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0103");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0104");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0105");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0106");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0107");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0108");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0109");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0110");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0111");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0112");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0113");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0114");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0115");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0116");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0117");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0118");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0119");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0120");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0121");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0122");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0123");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0124");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0125");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0126");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0127");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0128");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0129");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0130");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0131");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0132");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0133");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0134");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0135");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0136");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0137");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0138");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0139");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0140");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0141");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0142");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0143");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0144");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0145");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0146");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0147");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0148");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0149");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0150");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0151");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0152");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0153");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0154");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0155");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0156");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0157");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0158");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0159");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0160");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0161");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0162");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0163");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0164");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0165");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0166");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0167");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0168");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0169");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0170");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0171");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0172");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0173");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0174");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0175");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0176");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0177");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0178");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0179");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0180");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0181");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0182");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0183");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0184");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0185");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0186");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0187");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0188");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0189");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0190");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0191");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0192");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0193");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0194");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0195");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0196");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0197");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0198");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0199");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0200");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0201");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0202");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0203");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0204");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0205");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0206");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0207");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0208");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0209");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0210");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0211");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0212");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0213");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0214");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0215");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0216");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0217");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0218");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0219");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0220");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0221");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0222");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0223");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0224");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0225");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0226");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0227");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0228");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0229");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0230");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0231");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0232");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0233");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0234");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0235");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0236");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0237");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0238");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0239");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0240");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0241");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0242");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0243");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0244");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0245");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0246");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0247");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0248");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0249");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0250");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0251");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0252");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0253");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0254");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0255");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0256");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0257");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0258");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0259");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0260");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0261");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0262");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0263");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0264");

            migrationBuilder.DeleteData(
                table: "TBService",
                keyColumn: "Code",
                keyValue: "TBS0265");

            migrationBuilder.AddColumn<Guid>(
                name: "HospitalId",
                table: "Notification",
                nullable: true);
        }
    }
}
