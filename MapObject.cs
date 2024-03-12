using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace WindowsFormsApp1
{
    [Serializable]
    class MapObject
    {
        //Palette palette = new Palette();
        public string rebm = "";
        public int ver = 0;

        public List<MapLayer> map_layers = new List<MapLayer>();
        public List<Material> materials = new List<Material>();
        public List<Entity> entities = new List<Entity>();
        public List<Block> blocks_list = new List<Block>();

        public IEnumerable<byte> audio;
        public IEnumerable<byte> u1;
        public IEnumerable<byte> u2;
        public IEnumerable<byte> u3;
        public IEnumerable<byte> u4;
        public IEnumerable<byte> u5;

        public int bounds_minx, bounds_maxx;
        public int bounds_miny, bounds_maxy;
        public int bounds_minz, bounds_maxz;

        //
        // pressOK & k0 stuff
        //
        public void AddMapLayer(int height, List<Tuple<int, int>> points)
        {
            MapLayer m = new MapLayer(height, points);
            this.map_layers.Add(m);
        }
        public void AddMaterial(string name)
        {
            bool exists = false;
            foreach (Material m in this.materials)
            {
                if (m.name == name)
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                Material m = new Material(name);
                this.materials.Add(m);
            }
        }
        public void AddEntity(
                string name,
                float x, float y, float z,
                float xrot = 0.0f, float yrot = 0.0f, float zrot = 0.0f,
                float xscale = 0.0f, float yscale = 0.0f, float zscale = 0.0f,
                List<Tuple<string, string>> properties = null)
        {
            Entity e = new Entity(name, new Vector3(x, y, z), new Vector3(xrot, yrot, zrot), new Vector3(xscale, yscale, zscale), properties);
            this.entities.Add(e);
        }
        public void RemoveEntitiy(string name)
        {
            foreach (Entity e in this.entities)
            {
                if (e.name == name)
                {
                    this.entities.Remove(e);
                    break;
                }
            }
        }
        public void AddBlock(Block b)
        {
            this.blocks_list.Add(b);
        }
        public Block MakeBlock(Vector3 position, Dictionary<string, int> mats)
        {
            int type = 1;
            int orient = 1;
            /*Dictionary<string, int> mats = mats; 
            new Dictionary<string, int>()
            {
                ["left"] = 1,
                ["right"] = 1,
                ["front"] = 1,
                ["back"] = 1,
                ["top"] = 1,
                ["bottom"] = 1
            };*/
            Dictionary<string, Dictionary<string, int>> material_offsets = null;
            byte[] u1 = null;
            byte[] u2 = null;
            byte[] u3 = null;

            Block b = new Block(position, type, u1, mats, u2, material_offsets, orient, u3);
            return b;
        }

        //
        // Generator
        //
        public void GenerateMaze(int roomCount, bool roomRandomApproach, bool upramps, bool downramps, float roomSizeMultiply,
                    int[] matids, bool bevels,
                    bool roomOutlinesFlag, string roomOutlinesMaterial, string roomOutlinesColor1, string roomOutlinesColor2, string roomOutlinesColor3,
                    bool rampOutlinesFlag, string rampOutlinesMaterial, string rampOutlinesColor1, string rampOutlinesColor2, string rampOutlinesColor3,
                    bool rampPropFlag, string rampPropMaterial, string rampPropColor1, string rampPropColor2, string rampPropColor3,
                    bool rampBillboardsFlag, string rampBillboardsColor,
                    bool startEndBillboardsFlag, string startEndBillboardsColor,
                    bool routeArrowsFlag, string routeArrowsColor,
                    bool pointLightsFlag, string pointLightsColor,
                    bool skyboxFlag, string skyboxName, string accent1,
                    bool bfDecal, string bfDecalPath, string bfDecalColor,
                    bool shadowAmbient, string shadowAmbientColor,
                    bool fog, string fogColor,
                    bool ambient, string ambientColor,
                    bool sun, string sunColor,
                    bool rl, bool dj, bool haste, bool mj, string gravity)
        {
            Random random = new Random();

            // Clearing up template file 
            this.blocks_list = new List<Block>();
            this.entities = new List<Entity>();

            // Blocks (Route)
            Vector3 startingPoint       = new Vector3(random.Next(200, 300), random.Next(200, 300), -350);
            List<Vector3> roomsSizeList = RoomsSizeList(roomCount, roomSizeMultiply, random);

            List<List<Vector3>> roomsCoordsList = RoomsCoordsList(roomCount, startingPoint, roomsSizeList, roomRandomApproach, routeArrowsFlag, routeArrowsColor);
            List<Block> blocksToBuild           = CalculateBlocks(roomsCoordsList, matids, roomCount);

            foreach (Block block in blocksToBuild)
            {
                AddBlock(block);
            }

            // Entities 
            CreateArt(roomsCoordsList, roomCount, blocksToBuild,
                upramps, downramps,
                roomOutlinesFlag, rampOutlinesFlag, rampPropFlag, rampBillboardsFlag, pointLightsFlag,
                roomOutlinesMaterial, roomOutlinesColor1, roomOutlinesColor2, roomOutlinesColor3,
                rampOutlinesMaterial, rampOutlinesColor1, rampOutlinesColor2, rampOutlinesColor3,
                rampPropMaterial, rampPropColor1, rampPropColor2, rampPropColor3,
                rampBillboardsColor, pointLightsColor);
            CreateEntitiesBasics(roomsCoordsList, roomsSizeList, skyboxFlag, skyboxName, accent1);
            CreateEntitiesLights(roomsCoordsList, shadowAmbient, shadowAmbientColor, fog, fogColor, ambient, ambientColor, sun, sunColor);

            // Logic
            CreateLogicPhysics(roomsCoordsList, upramps, gravity, dj, mj);            
            if (haste)
                CreateLogicHaste(roomsCoordsList);
            CreateLogicOnConnect(roomsCoordsList);
            CreateLogicOnStart(roomsCoordsList, rl, haste);
            CreateLogicOnRestart(roomsCoordsList, haste, rl);
            CreateLogicOnGameLaunch(roomsCoordsList);
            CreateLogicChatCommands(roomsCoordsList);

            // Calculating Min/Max XYZ
            Vector3 min = new Vector3(roomsCoordsList.Min(coord => coord[0].X), roomsCoordsList.Min(coord => coord[0].Y), roomsCoordsList.Min(coord => coord[0].Z));
            Vector3 max = new Vector3(roomsCoordsList.Max(coord => coord[1].X), roomsCoordsList.Max(coord => coord[1].Y), roomsCoordsList.Max(coord => coord[1].Z));
            

            // Billboards
            CreateBillboards(roomsCoordsList, roomsSizeList, startEndBillboardsFlag, startEndBillboardsColor);

            // Decals
            if (bevels)
                CreateBevels(startingPoint, min, max);
            if (bfDecal)
                CreateBFDecal(startingPoint, min, max, bfDecalPath, bfDecalColor);
            CreateDecals(roomsCoordsList, rl);
                    

            // Splits
            CreateSplits(roomCount, min, max, blocksToBuild);  

        }

        public bool CheckRoomOverlap(Vector3 block, int roomIndex, List<List<Vector3>> roomCoordsList)
        {
            foreach (var roomCoords in roomCoordsList)
            {
                if (ReferenceEquals(roomCoords, roomCoordsList[roomIndex]))
                    continue; // Skip the current room using reference comparison

                Vector3 roomStart = roomCoords[0];
                Vector3 roomEnd = roomCoords[1];

                if (block.X >= roomStart.X && block.X <= roomEnd.X
                    && block.Y >= roomStart.Y && block.Y <= roomEnd.Y
                    && block.Z >= roomStart.Z && block.Z <= roomEnd.Z)
                {
                    return true; // Collision detected
                }
            }

            return false; // No collision detected
        }

        public Vector3 GenerateRooms(Random random, double roomSizeMultiply, bool isFirstRoom)
        {
            int width, height, depth;
            int form = random.Next(3);
            
            width   = (int)Math.Round(random.Next(18, 42) * roomSizeMultiply / 2) * 2;
            height  = (int)Math.Round(random.Next(14, 28) * roomSizeMultiply / 2) * 2;
            depth   = (int)Math.Round(random.Next(18, 42) * roomSizeMultiply / 2) * 2;
            
            switch (form)
            {
                case 0: // длинный по X
                    depth /= 2;
                    break;
                case 1: // длинный по Z
                    width /= 2;
                    break;
                case 2:
                    break;
                default:
                    width /= 2;
                    break;
            }
            if (isFirstRoom)
            {
                width = (int)Math.Round(random.Next(18, 24) * roomSizeMultiply / 2) * 2;
                if (width % 2 == 0)
                {
                    width += 1;
                }
                
            }

            return new Vector3(width, height, depth);
        }

        public List<Block> CalculateBlocks(List<List<Vector3>> roomCoordsList, int[] matids, int roomCount)
        {
            List<Block> result = new List<Block>();
            
            foreach (List<Vector3> roomCoords in roomCoordsList)
            {
                Vector3 roomStart   = roomCoords[0];
                Vector3 roomEnd     = roomCoords[1];

                // Generating blocks for each room
                for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
                {
                    for (float x = roomStart.X; x <= roomEnd.X; x++)
                    {
                        for (float y = roomStart.Y; y <= roomEnd.Y; y++)
                        {
                            for (float z = roomStart.Z; z <= roomEnd.Z; z++)
                            {
                                // Checking if block is on Room Frame (to build hollow rooms)
                                bool isOnRoomBorder = x == roomStart.X || x == roomEnd.X || y == roomStart.Y || y == roomEnd.Y || z == roomStart.Z || z == roomEnd.Z;                                
                                if (isOnRoomBorder)
                                {
                                    // Checking if block overlaping other rooms (to make route accessible)                                    
                                    bool isInsideLastRooms = CheckRoomOverlap(new Vector3(x, y, z), roomIndex, roomCoordsList);

                                    if (!isInsideLastRooms)
                                    {
                                        Vector3 currentBlock = new Vector3(x, y, z);
                                        Dictionary<string, int> mats = PaintBlock(currentBlock, roomIndex, roomStart, roomEnd, roomCount, matids);
                                        foreach (Vector3 thiccBlock in Thicc(currentBlock))
                                        {                                            
                                            if (thiccBlock.X < roomStart.X || thiccBlock.X > roomEnd.X || thiccBlock.Y < roomStart.Y || thiccBlock.Y > roomEnd.Y || thiccBlock.Z < roomStart.Z || thiccBlock.Z > roomEnd.Z)
                                            {
                                                Block block = MakeBlock(new Vector3(thiccBlock.X, thiccBlock.Y, thiccBlock.Z), mats);
                                                result.Add(block);
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return result;
        }

        public void CreateUpRamp(Vector3 roomStart, Vector3 roomEnd, Vector3 roomStartNext, int yUpDiff, List<Block> blocksToBuild, int roomIndex,
            string rampOutlinesMaterial, string rampOutlinesColor1, string rampOutlinesColor2, string rampOutlinesColor3,
            string rampPropMaterial, string rampPropColor1, string rampPropColor2, string rampPropColor3,
            string rampBillboardsColor, 
            bool rampOutlinesFlag, bool rampPropFlag, bool rampBillboardsFlag)
        {
            //checking if starting block for ramp exists
            Vector3 rampBlock = new Vector3(roomEnd.X - 1, roomStart.Y - 1, roomEnd.Z - yUpDiff);
            rampBlock.Z = rampBlock.Z > roomStartNext.Z ?
                roomStartNext.Z + yUpDiff + 2 :
                rampBlock.Z ;
            Vector3 newRampStart    = roomStart;
            Vector3 newRampEnd      = rampBlock;
            bool blockExists        = false;

            // calculating ramp width
            for (int z = (int)rampBlock.Z; z <= rampBlock.Z + yUpDiff; z++)
            {
                for (int x = (int)roomStart.X; x <= roomEnd.X; x++)
                {
                    // checking every block underneath ramp
                    bool currentBlockExist = BlockExists(new Vector3(x, rampBlock.Y, z), blocksToBuild);

                    if (currentBlockExist)
                    {
                        if (blockExists)
                        {
                            newRampEnd.X = x;
                        }
                        else
                        {
                            newRampStart.X = x;
                            blockExists = true;
                        }
                    }
                    else
                    {
                        if (blockExists)
                        {
                            break;
                        }
                        blockExists = false;
                    }
                }
            }
            Vector3 positionBlock = new Vector3(newRampEnd.X, rampBlock.Y, rampBlock.Z);
            bool positionExist = BlockExists(positionBlock, blocksToBuild);

            if (!blockExists)
            {  
                float limit = positionBlock.X - 50;
                if (!positionExist)
                {
                    do
                    {
                        if (positionBlock.X > limit)
                        {
                            positionBlock = new Vector3(positionBlock.X - 1, positionBlock.Y, positionBlock.Z);
                            positionExist = BlockExists(positionBlock, blocksToBuild);
                        }
                        else break;
                    }
                    while (!positionExist);

                }
            }

            if (blockExists)
            { 
                float abs = Math.Abs(positionBlock.X - newRampStart.X) - 1;
                if (abs > 1 && positionExist)
                {
                    List<Tuple<string, string>> billboardEntityProperties = new List<Tuple<string, string>>{
                            new Tuple<string, string>("alpha", "false"),
                            new Tuple<string, string>("backface", "true"),
                            new Tuple<string, string>("color", rampBillboardsColor)
                    };
                    double thetaRadians = Math.Atan(yUpDiff / yUpDiff * 2);
                    double thetaDegrees = thetaRadians * (180.0 / Math.PI);

                    // physical ramp
                    Vector3 position = new Vector3(positionBlock.X * 40, (positionBlock.Y + 1) * 20, positionBlock.Z * 40);
                    Vector3 rotation = new Vector3(0, 0, 90);
                    Vector3 scale = new Vector3(yUpDiff / 2f, abs, yUpDiff);
                    List<Tuple<string, string>> entityProperties = new List<Tuple<string, string>> {
                        new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal")};
                    this.entities.Add(new Entity($"prop_ramp_up_{roomIndex}", position, rotation, scale, entityProperties));


                    // visible to players ramp
                    if (rampPropFlag)
                    {
                        entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal"),
                            new Tuple<string, string>("material", rampPropMaterial),
                            new Tuple<string, string>("color", rampPropColor1),
                            new Tuple<string, string>("color2", rampPropColor2),
                            new Tuple<string, string>("color3", rampPropColor3),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                            };
                        this.entities.Add(new Entity($"prop_ramp_up_{roomIndex}_visible", position, rotation, scale, entityProperties));
                    }

                    
                    if (rampOutlinesFlag)
                    {
                        // internal ramp (art)
                        scale = new Vector3(yUpDiff / 4f, abs - 2, yUpDiff / 2f);
                        position = new Vector3((positionBlock.X - 1) * 40, (positionBlock.Y + scale.X + 0.5f) * 20, (positionBlock.Z + scale.X + 0.25f) * 40);
                        entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal"),
                            new Tuple<string, string>("material", rampOutlinesMaterial),
                            new Tuple<string, string>("color", rampOutlinesColor1),
                            new Tuple<string, string>("color2", rampOutlinesColor2),
                            new Tuple<string, string>("color3", rampOutlinesColor3),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                        };
                        this.entities.Add(new Entity($"prop_ramp_up_{roomIndex}_internal", position, rotation, scale, entityProperties));

                        entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("model", "invisible_opaque_box"),
                            new Tuple<string, string>("material", rampOutlinesMaterial),
                            new Tuple<string, string>("color", rampOutlinesColor1),
                            new Tuple<string, string>("color2", rampOutlinesColor2),
                            new Tuple<string, string>("color3", rampOutlinesColor3),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                        };
                        //horizontal
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampStart.X + 1, positionBlock.Y + 1, positionBlock.Z), new Vector3(0, 0, 0), entityProperties, 2, 3, "rampoutline_front", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(newRampStart.X + 1, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 2, 3, "rampoutline_lowerback", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3(newRampStart.X + 1, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 2, 3, "rampoutline_higherback", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 3, 0, "rampoutline_lowerright", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X + 1, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampStart.X + 1, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 0, 0, "rampoutline_lowerleft", rampOutlinesColor3);

                        // vertical outlines
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(newRampEnd.X, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 2, 6, "rampoutline_lowhighright", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X + 1, positionBlock.Y + 1, newRampEnd.Z + yUpDiff), new Vector3(newRampStart.X + 1, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3(0, 0, 0), entityProperties, 1, 5, "rampoutline_lowhighleft", rampOutlinesColor3);
                        //angular
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampEnd.X, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3((float)thetaDegrees, 0, 0), entityProperties, 3, 6, "rampoutline_angle_right", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X + 1, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampStart.X + 1, positionBlock.Y + 1 + yUpDiff, newRampEnd.Z + yUpDiff), new Vector3((float)thetaDegrees, 0, 0), entityProperties, 0, 5, "rampoutline_angle_left", rampOutlinesColor3);
                    }

                    
                    if (rampBillboardsFlag)
                    {
                        //back billboard
                        position = new Vector3(newRampEnd.X * 40, (positionBlock.Y + 1) * 20, (newRampEnd.Z + yUpDiff) * 40);
                        rotation = new Vector3(0, 180, 0);
                        scale = new Vector3(abs * 40, yUpDiff * 20, 1);
                        
                        this.entities.Add(new Entity($"billboard_ramp_up_{roomIndex}_back", position, rotation, scale, billboardEntityProperties));

                        // floor billboard
                        rotation = new Vector3(90, 0, 180);
                        scale = new Vector3(abs * 40, yUpDiff * 40, 1);
                        position.Y += 0.5f;
                        this.entities.Add(new Entity($"billboard_ramp_up_{roomIndex}_floor", position, rotation, scale, billboardEntityProperties));

                        // angled billboard
                        rotation = new Vector3((float)thetaDegrees, 0, 180);
                        scale = new Vector3(abs * 40, (int)Math.Sqrt(yUpDiff * yUpDiff * 40 * 40 + yUpDiff * yUpDiff * 20 * 20), 1);
                        position.Y = (positionBlock.Y + 1 + yUpDiff) * 20;
                        this.entities.Add(new Entity($"billboard_ramp_up_{roomIndex}_angle", position, rotation, scale, billboardEntityProperties));
                    }
                }
            }
        }

        public void CreateDownRamp(Vector3 roomStart, Vector3 roomStartNext, Vector3 roomEndNext, int yDownDiff, List<Block> blocksToBuild, int roomIndex,
            string rampOutlinesMaterial, string rampOutlinesColor1, string rampOutlinesColor2, string rampOutlinesColor3,
            string rampPropMaterial, string rampPropColor1, string rampPropColor2, string rampPropColor3,
            string rampBillboardsColor,
            bool rampOutlinesFlag, bool rampPropFlag, bool rampBillboardsFlag)
        {
            Vector3 rampBlock       = new Vector3(roomStartNext.X + 1, roomStartNext.Y - 1, roomStartNext.Z + yDownDiff * 2 + 1);
            Vector3 newRampStart    = roomStart;
            Vector3 newRampEnd      = rampBlock;
            bool blockExists = false;

            // calculating ramp width
            for (int z = (int)roomStartNext.Z; z <= rampBlock.Z; z++)
            {
                for (int x = (int)roomStartNext.X; x <= roomEndNext.X; x++)
                {
                    // checking every block underneath ramp
                    bool currentBlockExist = BlockExists(new Vector3(x, rampBlock.Y, z), blocksToBuild);

                    if (currentBlockExist)
                    {
                        if (blockExists)
                        {
                            newRampStart.X = x;
                        }
                        else
                        {
                            newRampEnd.X = x + 1;
                            blockExists = true;
                        }
                    }
                    else
                    {
                        if (blockExists)
                        {
                            break;
                        }
                        blockExists = false;
                    }
                }
            }
            if (blockExists)
            {
                float abs = Math.Abs(newRampEnd.X - newRampStart.X);
                Vector3 positionBlock = new Vector3(newRampEnd.X, rampBlock.Y, rampBlock.Z);
                Vector3 billboardPosition, billboardRotation, billboardScale;
                if (abs > 1 && BlockExists(positionBlock, blocksToBuild))
                {
                    List<Tuple<string, string>> billboardEntityProperties = new List<Tuple<string, string>>{
                        new Tuple<string, string>("alpha", "false"),
                        new Tuple<string, string>("backface", "true"),
                        new Tuple<string, string>("color", rampBillboardsColor)
                    };
                    double thetaRadians = Math.Atan(yDownDiff / yDownDiff * 4);
                    double thetaDegrees = thetaRadians * (180.0 / Math.PI);

                    // physical ramp
                    Vector3 position = new Vector3(positionBlock.X * 40, (positionBlock.Y + 1) * 20, positionBlock.Z * 40);
                    Vector3 rotation = new Vector3(0, 180, 90);
                    Vector3 scale = new Vector3(yDownDiff / 2f, abs, yDownDiff * 2f);
                    List<Tuple<string, string>> entityProperties = new List<Tuple<string, string>> {
                        new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal")
                    };
                    this.entities.Add(new Entity($"prop_ramp_down_{roomIndex}", position, rotation, scale, entityProperties));

                    // visible to players ramp
                    if (rampPropFlag)
                    {
                        entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal"),
                            new Tuple<string, string>("material", rampPropMaterial),
                            new Tuple<string, string>("color", rampPropColor1),
                            new Tuple<string, string>("color2", rampPropColor2),
                            new Tuple<string, string>("color3", rampPropColor3),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                            };
                        this.entities.Add(new Entity($"prop_ramp_down_{roomIndex}_visible", position, rotation, scale, entityProperties));
                    }

                    if (rampOutlinesFlag)
                    {
                        // internal ramp (art)
                        scale = new Vector3(yDownDiff / 4f, abs - 2, yDownDiff);
                        position = new Vector3((positionBlock.X + 1) * 40, (positionBlock.Y + scale.X + 0.5f) * 20, (positionBlock.Z - scale.Z / 2 - 0.25f) * 40);
                        entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("model", "props/invisible/invisible_block_diagonal/invisible_block_diagonal"),
                            new Tuple<string, string>("material", rampOutlinesMaterial),
                            new Tuple<string, string>("color", rampOutlinesColor1),
                            new Tuple<string, string>("color2", rampOutlinesColor2),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                        };
                        this.entities.Add(new Entity($"prop_ramp_down_{roomIndex}_internal", position, rotation, scale, entityProperties));


                        entityProperties = new List<Tuple<string, string>> { 
                            new Tuple<string, string>("model", "invisible_opaque_box"),
                            new Tuple<string, string>("material", rampOutlinesMaterial),
                            new Tuple<string, string>("color", rampOutlinesColor1),
                            new Tuple<string, string>("color2", rampOutlinesColor2),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true"),
                        };
                        // horizontal
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampStart.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(0, 0, 0), entityProperties, 0, 1, "rampoutline_front", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampStart.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(0, 0, 0), entityProperties, 0, 1, "rampoutline_lowerback", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampStart.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3(0, 0, 0), entityProperties, 2, 1, "rampoutline_higherback", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampStart.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(0, 0, 0), entityProperties, 3, 0, "rampoutline_lowerright", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(0, 0, 0), entityProperties, 0, 0, "rampoutline_lowerleft", rampOutlinesColor3);

                        //vertical
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampEnd.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3(0, 0, 0), entityProperties, 4, 2, "rampoutline_lowhighright", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X, positionBlock.Y + 1, newRampEnd.Z - yDownDiff * 2), new Vector3(newRampStart.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3(0, 0, 0), entityProperties, 7, 2, "rampoutline_lowhighleft", rampOutlinesColor3);

                        //angular
                        CreateOutlineGroup(roomIndex, new Vector3(newRampEnd.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampEnd.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3((float)thetaDegrees, 180, 180), entityProperties, 6, 3, "rampoutline_angle_right", rampOutlinesColor3);
                        CreateOutlineGroup(roomIndex, new Vector3(newRampStart.X, positionBlock.Y + 1, positionBlock.Z), new Vector3(newRampStart.X, positionBlock.Y + 1 + yDownDiff, newRampEnd.Z - yDownDiff * 2), new Vector3((float)thetaDegrees, 180, 180), entityProperties, 5, 2, "rampoutline_angle_left", rampOutlinesColor3);
                    }

                    

                    if (rampBillboardsFlag)
                    {
                        //back billboard
                        billboardPosition = new Vector3(newRampEnd.X * 40, (positionBlock.Y + 1) * 20, (positionBlock.Z - yDownDiff * 2) * 40);
                        billboardRotation = new Vector3(0, 0, 0);
                        billboardScale = new Vector3(abs * 40, yDownDiff * 20, 1);                        
                        this.entities.Add(new Entity($"billboard_ramp_down_{roomIndex}_back", billboardPosition, billboardRotation, billboardScale, billboardEntityProperties));

                        //floor billboard
                        billboardRotation = new Vector3(90, 0, 0);
                        billboardScale = new Vector3(abs * 40, yDownDiff * 80, 1);
                        billboardPosition.Y += 0.5f;
                        this.entities.Add(new Entity($"billboard_ramp_up_{roomIndex}_floor", billboardPosition, billboardRotation, billboardScale, billboardEntityProperties));

                        //angled billboard
                        billboardRotation = new Vector3((float)thetaDegrees, 180, 180);
                        billboardScale = new Vector3(abs * 40, (int)Math.Sqrt(yDownDiff * yDownDiff * 80 * 80 + yDownDiff * yDownDiff * 20 * 20), 1);
                        billboardPosition.Y = (positionBlock.Y + 1 + yDownDiff) * 20;
                        this.entities.Add(new Entity($"billboard_ramp_down_{roomIndex}_angle", billboardPosition, billboardRotation, billboardScale, billboardEntityProperties));
                    }
                }
            }
        }

        public Vector3 CalculateRay(Vector3 startingPoint, List<Block> blocksToBuild, Vector3 endPoint)
        {
            Vector3 result = startingPoint;
            bool foundBlockInXDirection = false, foundBlockInYDirection = false, foundBlockInZDirection = false;

            float yLimit = startingPoint.Y + (100 * endPoint.Y);

            // Searching for min(= -1), none (= 0), max (= 1) in Y-axis
            for (float y = startingPoint.Y; y != yLimit; y += endPoint.Y)
            {
                if (BlockExists(new Vector3(result.X, y, result.Z), blocksToBuild))
                {
                    if (!foundBlockInYDirection)
                    {
                        foundBlockInYDirection = true;
                    }
                    result.Y = y;
                }
                else
                {
                    if (foundBlockInYDirection)
                    {
                        break;
                    }
                }
            }

            // calibration #1 Z for X search
            float newZ = result.Z;
            float xLimit = startingPoint.X + (100 * endPoint.X);
            if (endPoint.Z == 1) { newZ = result.Z - 1; }
            if (endPoint.Z == -1) { newZ = result.Z + 1; }


            // Searching for min(= -1), none (= 0), max (= 1) ray in X-axis
            for (float x = startingPoint.X; x != xLimit; x += endPoint.X)
            {
                bool outlineCalcFlag = BlockExists(new Vector3(x, result.Y, newZ), blocksToBuild);
                if (outlineCalcFlag)
                {
                    if (!foundBlockInXDirection)
                    {
                        foundBlockInXDirection = true;
                    }
                    result.X = x;
                }
                else
                {
                    break;
                }
            }

            // calibration X for Z search
            float newX = result.X;
            if (endPoint.X == 1)    { newX = result.X - 1; }
            if (endPoint.X == -1)   { newX = result.X + 1; }

            // Define the search direction and limit for Z-axis
            float zLimit = startingPoint.Z + (100 * endPoint.Z);

            // Searching for min(= -1), none (= 0), max (= 1) in Z-axis
            for (float z = startingPoint.Z; z != zLimit; z += endPoint.Z)
            {
                bool outlineCalcFlag;
                if (endPoint.X == 0)
                {
                    bool nearbyBlocksPlus   = BlockExists(new Vector3(result.X + 2, result.Y, z), blocksToBuild);
                    bool nearbyBlocksMinus  = BlockExists(new Vector3(result.X - 2, result.Y, z), blocksToBuild);
                    bool upperBlock         = BlockExists(new Vector3(newX, result.Y + 1, z), blocksToBuild);
                    outlineCalcFlag         = BlockExists(new Vector3(newX, result.Y, z), blocksToBuild) && 
                        (!nearbyBlocksMinus || !nearbyBlocksPlus) && !upperBlock;
                }
                else
                {
                    outlineCalcFlag = BlockExists(new Vector3(newX, result.Y, z), blocksToBuild);
                }

                if (outlineCalcFlag)
                {
                    if (!foundBlockInZDirection)
                    {
                        foundBlockInZDirection = true;
                    }
                    result.Z = z;
                }
                else
                {
                    if (foundBlockInZDirection && 
                        BlockExists(new Vector3(newX, result.Y, z), blocksToBuild) && 
                        !BlockExists(new Vector3(newX, result.Y, z + endPoint.Z), blocksToBuild) &&
                        !BlockExists(new Vector3(newX, result.Y + 1, z), blocksToBuild))
                    {
                        result.Z = z;
                    }
                    break;
                }
            }



            // calibration #2 Z for X-search
            newZ = result.Z;
            if (endPoint.Z == 1) { newZ = result.Z - 1; }
            if (endPoint.Z == -1) { newZ = result.Z + 1; }
            foundBlockInXDirection = false;

            // Define the search direction and limit for X-axis
            xLimit = result.X + (100 * endPoint.X);

            // Searching for min(= -1), none (= 0), max (= 1) in X-axis
            for (float x = result.X; x != xLimit; x += endPoint.X)
            {
                bool outlineCalcFlag;
                if (endPoint.Z == 0)
                {
                    bool nearbyBlocksPlus   = BlockExists(new Vector3(x, result.Y, result.Z + 1), blocksToBuild);
                    bool nearbyBlocksMinus  = BlockExists(new Vector3(x, result.Y, result.Z - 1), blocksToBuild);
                    bool upperBlock         = BlockExists(new Vector3(x, result.Y + 1, newZ), blocksToBuild);
                    outlineCalcFlag         = BlockExists(new Vector3(x, result.Y, newZ), blocksToBuild) && 
                        (!nearbyBlocksMinus || !nearbyBlocksPlus) && !upperBlock;
                }
                else
                {
                    outlineCalcFlag = BlockExists(new Vector3(x, result.Y, newZ), blocksToBuild);
                }

                if (outlineCalcFlag)
                {
                    if (!foundBlockInXDirection)
                    {
                        foundBlockInXDirection = true;
                    }
                    result.X = x;
                }
                else
                {
                    if (foundBlockInXDirection && 
                        BlockExists(new Vector3(x, result.Y, newZ), blocksToBuild) && 
                        !BlockExists(new Vector3(x + endPoint.X, result.Y, newZ), blocksToBuild) && 
                        !BlockExists(new Vector3(x, result.Y + 1, newZ), blocksToBuild))
                    {
                        result.X = x;
                    }
                    break;
                }
            }


            return result;
        }
       
        public void CreateArt(List<List<Vector3>> roomCoordsList, int roomCount, List<Block> blocksToBuild, 
            bool upramps, bool downramps, 
            bool roomOutlinesFlag, bool rampOutlinesFlag, bool rampPropFlag, bool rampBillboardsFlag, bool pointLightsFlag,
            string roomOutlinesMaterial, string roomOutlinesColor1, string roomOutlinesColor2, string roomOutlinesColor3,
            string rampOutlinesMaterial, string rampOutlinesColor1, string rampOutlinesColor2, string rampOutlinesColor3,
            string rampPropMaterial, string rampPropColor1, string rampPropColor2, string rampPropColor3,
            string rampBillboardsColor, string pointLightsColor)
        {            
            int allowedUpHeight     = 3;
            int allowedDownHeight   = 3;
            bool rampCreated = false;

            // Generating entities for each room
            for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
            {
                Vector3 roomStart   = roomCoordsList[roomIndex][0];
                Vector3 roomEnd     = roomCoordsList[roomIndex][1];
                Vector3 centerPoint = new Vector3(
                    (float)Math.Round((roomStart.X + roomEnd.X) / 2),
                    (float)Math.Round((roomStart.Y + roomEnd.Y) / 2),
                    (float)Math.Round((roomStart.Z + roomEnd.Z) / 2));

                if (roomIndex != roomCount - 1)
                {
                    Vector3 roomStartNext   = roomCoordsList[roomIndex + 1][0];
                    Vector3 roomEndNext     = roomCoordsList[roomIndex + 1][1];
                    //upramps & downramps
                    int yUpDiff         = (int)(roomStartNext.Y - roomStart.Y);
                    int yDownDiff       = (int)(roomStart.Y - roomStartNext.Y);
                    bool uprampCheck    = yUpDiff >= allowedUpHeight;
                    bool downrampCheck  = yDownDiff >= allowedDownHeight;
                    // checking if next room is *THAT* low/high enough to place ramps
                    if (uprampCheck && upramps)
                    {
                        CreateUpRamp(roomStart, roomEnd, roomStartNext, yUpDiff, blocksToBuild, roomIndex, 
                            rampOutlinesMaterial, rampOutlinesColor1, rampOutlinesColor2, rampOutlinesColor3,
                            rampPropMaterial, rampPropColor1, rampPropColor2, rampPropColor3,
                            rampBillboardsColor,
                            rampOutlinesFlag, rampPropFlag, rampBillboardsFlag);
                        rampCreated = true;
                    }
                    if (downrampCheck && downramps && !rampCreated)
                    {
                        CreateDownRamp(roomStart, roomStartNext, roomEndNext, yDownDiff, blocksToBuild, roomIndex,
                            rampOutlinesMaterial, rampOutlinesColor1, rampOutlinesColor2, rampOutlinesColor3,
                            rampPropMaterial, rampPropColor1, rampPropColor2, rampPropColor3,
                            rampBillboardsColor,
                            rampOutlinesFlag, rampPropFlag, rampBillboardsFlag);
                    }
                }

                
                List<Vector3> startingPointsFirst = new List<Vector3> {
                    CalculateRay(centerPoint, blocksToBuild, new Vector3(-1, -1, -1)),
                    CalculateRay(centerPoint, blocksToBuild, new Vector3(-1, -1, 1)),
                    CalculateRay(centerPoint, blocksToBuild, new Vector3(1 , -1, 1)),
                    CalculateRay(centerPoint, blocksToBuild, new Vector3(1 , -1, -1))};
                
                List<Vector3> lowerXZSides = new List<Vector3> {
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, -1),
                    new Vector3(-1, 0, 0),};

                List<Vector3> startingPoints = new List<Vector3>(startingPointsFirst);
                /*foreach (Vector3 startingPoint in startingPointsFirst)
                {
                    foreach (Vector3 side in lowerXZSides)
                    {
                        Vector3 newStartingPoint = CalculateRay(startingPoint, blocksToBuild, side);
                        if (newStartingPoint != startingPoint && !startingPoints.Contains(newStartingPoint)) { startingPoints.Add(newStartingPoint); }

                    }
                }*/

                if (roomOutlinesFlag)
                {
                    CreateOutlines(roomIndex, blocksToBuild, startingPoints, lowerXZSides, roomOutlinesMaterial, roomOutlinesColor1, roomOutlinesColor2, roomOutlinesColor3);           
                }
                if (pointLightsFlag)
                {
                    CreateMiddleLight(roomIndex, centerPoint, startingPoints, pointLightsColor);
                }
            }            
        }

        public bool BlockExists(Vector3 rampBlock, List<Block> blocksToBuild)
        {
            return blocksToBuild.Any(block =>                 
                block.x == rampBlock.X &&
                block.y == rampBlock.Y && 
                block.z == rampBlock.Z);
        }

        public void CreateMiddleLight(int roomIndex, Vector3 centerPoint, List<Vector3> startingPoints, string pointLightsColor)
        {
            //middle lights
            Vector3 middleLightPosition = new Vector3(centerPoint.X * 40, centerPoint.Y * 20, centerPoint.Z * 40);
            float radius = (startingPoints[2].Z - startingPoints[0].Z) > (startingPoints[2].X - startingPoints[0].X) ?
                (startingPoints[2].X - startingPoints[0].X) * 60 :
                (startingPoints[2].Z - startingPoints[0].Z) * 60;
            List<Tuple<string, string>> middleLightProperties = new List<Tuple<string, string>> {
                new Tuple<string, string>("color", pointLightsColor),
                new Tuple<string, string>("intensity", "8"),
                new Tuple<string, string>("type", "diffuse"),
                new Tuple<string, string>("radius", $"{radius}"),};
            this.entities.Add(new Entity($"light_room_{roomIndex}_middle", middleLightPosition, new Vector3(0, 0, 0), new Vector3(1, 1, 1), middleLightProperties));
        }

        public void CreateOutlineLight(int roomIndex, Vector3 firstPoint, Vector3 nextPoint, int indexSP, int indexNP, Vector3 scale, string outlineLightColor)
        {
            //outline lights
            Vector3 positionLight = new Vector3(firstPoint.X * 40, firstPoint.Y * 20, firstPoint.Z * 40);
            Vector3 rotationLight = new Vector3(0, 0, 0);
            float scaleLight = Math.Abs(firstPoint.X - nextPoint.X) > Math.Abs(firstPoint.Z - nextPoint.Z) ? scale.X : scale.Z;
            List<Tuple<string, string>> entityPropertiesLight = new List<Tuple<string, string>> {
                new Tuple<string, string>("color", outlineLightColor),
                new Tuple<string, string>("intensity", "40"),
                new Tuple<string, string>("length", $"{scaleLight * 100 }"),
                new Tuple<string, string>("radius", "200"),
                new Tuple<string, string>("falloff", "0.1"),
                new Tuple<string, string>("type", "capsule")};
            if (indexNP == 0)
            {
                positionLight = indexSP == 0 || indexSP == 1 ?
                    new Vector3(positionLight.X + 5, positionLight.Y + 5, positionLight.Z) :
                    new Vector3(positionLight.X - 5, positionLight.Y + 5, positionLight.Z);
            }
            if (indexNP == 1)
            {
                rotationLight = new Vector3(0, 90, 0);
                positionLight = indexSP == 1 || indexSP == 2 ?
                    new Vector3(positionLight.X, positionLight.Y + 5, positionLight.Z - 5) :
                    new Vector3(positionLight.X, positionLight.Y + 5, positionLight.Z + 5);
            }
            if (indexNP == 2)
            {
                rotationLight = new Vector3(0, 180, 0);
                positionLight = indexSP == 2 || indexSP == 3 ?
                    new Vector3(positionLight.X - 5, positionLight.Y + 5, positionLight.Z) :
                    new Vector3(positionLight.X + 5, positionLight.Y + 5, positionLight.Z);
            }
            if (indexNP == 3)
            {
                rotationLight = new Vector3(0, 270, 0);
                positionLight = indexSP == 0 || indexSP == 3 ?
                    new Vector3(positionLight.X, positionLight.Y + 5, positionLight.Z + 5) :
                    new Vector3(positionLight.X, positionLight.Y + 5, positionLight.Z - 5);
            }

            this.entities.Add(new Entity($"light_room_{roomIndex}_outline_{indexSP}_{indexNP}", positionLight, rotationLight, new Vector3(1, 1, 1), entityPropertiesLight));
        }

        public void CreateOutlineGroup(int roomIndex, Vector3 firstPoint, Vector3 nextPoint, Vector3 rotation, List<Tuple<string, string>> entityProperties, int indexSP, int indexNP, string usedFor, string outlineLightColor)
        { 
            Vector3 position = new Vector3(
                (firstPoint.X + nextPoint.X) * 20,
                (firstPoint.Y + nextPoint.Y) * 10,
                (firstPoint.Z + nextPoint.Z) * 20);
            Vector3 scale = new Vector3(0.05f, 0.05f, 0.05f);
            
            // Y-Axis outline
            if (firstPoint.Y != nextPoint.Y)
            {
                if (firstPoint.X == nextPoint.X && firstPoint.Z == nextPoint.Z)
                {
                    scale.Y = Math.Abs(nextPoint.Y - firstPoint.Y) / 5f + 0.05f;
                }
                if (firstPoint.Z != nextPoint.Z)
                {
                    if (indexSP == 5 || indexSP == 6)
                    {
                        scale.Y = (float)Math.Sqrt((Math.Abs(nextPoint.Y - firstPoint.Y) / 5f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 5f) + (Math.Abs(nextPoint.Y - firstPoint.Y) / 1.25f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 1.25f));
                    }
                    else
                    {
                        scale.Y = (float)Math.Sqrt((Math.Abs(nextPoint.Y - firstPoint.Y) / 5f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 5f) + (Math.Abs(nextPoint.Y - firstPoint.Y) / 2.5f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 2.5f));
                    }

                }
                if (firstPoint.X != nextPoint.X)
                {
                    scale.Y = (float)Math.Sqrt((Math.Abs(nextPoint.Y - firstPoint.Y) / 5f + 0.05f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 5f + 0.05f) + (Math.Abs(nextPoint.Y - firstPoint.Y) / 2.5f + 0.05f) * (Math.Abs(nextPoint.Y - firstPoint.Y) / 2.5f + 0.05f));
                }
            }
            // X-Axis, Z-Axis outline
            else
            {
                if (firstPoint.X == nextPoint.X) 
                {
                    scale.Z = Math.Abs(nextPoint.Z - firstPoint.Z) / 2.5f + 0.05f;
                } 
                else
                {
                    scale.X = Math.Abs(nextPoint.X - firstPoint.X) / 2.5f + 0.05f;
                }
            }
            

            if (!this.entities.Any(entity => entity.position == position && entity.scale == scale))
            {
                // outline (middle)
                this.entities.Add(new Entity($"prop_room_{roomIndex}_{usedFor}_{indexSP}_{indexNP}", position, rotation, scale, entityProperties));

                // capsule light
                if (indexNP < 4 && indexSP < 4)
                {
                    CreateOutlineLight(roomIndex, firstPoint, nextPoint, indexSP, indexNP, scale, outlineLightColor);
                }

                // first cube prop (start of outline)
                Vector3 positionCube = new Vector3(firstPoint.X * 40, firstPoint.Y * 20, firstPoint.Z * 40);
                Vector3 scaleCube = new Vector3(0.1f, 0.1f, 0.1f);
                if (!this.entities.Any(entity => entity.position == positionCube && entity.scale == scaleCube))
                {
                    this.entities.Add(new Entity($"prop_room_{roomIndex}_{usedFor}_cube_start_{indexSP}_{indexNP}", positionCube, rotation, scaleCube, entityProperties));
                }

                // second cube prop (end of outline)
                Vector3 positionNextCube = new Vector3(nextPoint.X * 40, nextPoint.Y * 20, nextPoint.Z * 40);
                if (!this.entities.Any(entity => entity.position == positionNextCube && entity.scale == scaleCube))
                {
                    this.entities.Add(new Entity($"prop_room_{roomIndex}_{usedFor}_cube_end_{indexSP}_{indexNP}", positionNextCube, rotation, scaleCube, entityProperties));
                }
            }
        }

        public void CreateOutlines(int roomIndex, List<Block> blocksToBuild, List<Vector3> startingPoints, List<Vector3> lowerXZSides, string outlineMaterial, string color1, string color2, string color3)
        {              
            foreach (Vector3 startingPoint in startingPoints)
            {
                int indexSP         = startingPoints.IndexOf(startingPoint);
                Vector3 firstPoint  = startingPoint;

                foreach (Vector3 side in lowerXZSides)
                {
                    firstPoint  = startingPoint;
                    int indexNP = lowerXZSides.IndexOf(side);
                    // checking which one is more suitable (=short)
                    Vector3 defaultNextPoint = CalculateRay(firstPoint, blocksToBuild, side);


                    // start manipulations on calc
                    if ((indexSP == 1 || indexSP == 2) && (indexNP == 1 || indexNP == 3)) { firstPoint.Z--; }
                    if ((indexSP == 0 || indexSP == 3) && (indexNP == 1 || indexNP == 3)) { firstPoint.Z++; }
                    if ((indexSP == 0 || indexSP == 1) && (indexNP == 0 || indexNP == 2)) { firstPoint.X++; }
                    if ((indexSP == 2 || indexSP == 3) && (indexNP == 0 || indexNP == 2)) { firstPoint.X--; }

                    Vector3 nextPoint = CalculateRay(firstPoint, blocksToBuild, side);

                    // end manipulations on calc
                    if ((indexSP == 1 || indexSP == 2) && (indexNP == 1 || indexNP == 3)) { firstPoint.Z++; nextPoint.Z++; }
                    if ((indexSP == 0 || indexSP == 3) && (indexNP == 1 || indexNP == 3)) { firstPoint.Z--; nextPoint.Z--; }
                    if ((indexSP == 0 || indexSP == 1) && (indexNP == 0 || indexNP == 2)) { firstPoint.X--; nextPoint.X--; }
                    if ((indexSP == 2 || indexSP == 3) && (indexNP == 0 || indexNP == 2)) { firstPoint.X++; nextPoint.X++; }



                    if (((indexNP == 1) && defaultNextPoint.X < nextPoint.X) || ((indexNP == 3) && defaultNextPoint.X > nextPoint.X))
                    { 
                        nextPoint.X = defaultNextPoint.X; 
                    }
                    
                    if (((indexNP == 2) && defaultNextPoint.Z > nextPoint.Z) || ((indexNP == 0) && defaultNextPoint.Z < nextPoint.Z))
                    {
                        nextPoint.Z = defaultNextPoint.Z; 
                    }

                    if (nextPoint != firstPoint)
                    {
                        // first point
                        if (indexSP == 1) { firstPoint.Z++; nextPoint.Z++; }
                        if (indexSP == 2) { firstPoint.X++; firstPoint.Z++; nextPoint.X++; nextPoint.Z++; }
                        if (indexSP == 3) { firstPoint.X++; nextPoint.X++; }
                        firstPoint.Y++;

                        // next point
                        if (indexNP == 0) { nextPoint.Z++; }
                        if (indexNP == 1) { nextPoint.X++; }
                        if (indexNP == 2) { nextPoint.Z--; }
                        if (indexNP == 3) { nextPoint.X--; }
                        nextPoint.Y++;

                        List<Tuple<string, string>> entityProperties = new List<Tuple<string, string>> {
                            new Tuple<string, string>("color", $"{color1}"),
                            new Tuple<string, string>("color2", $"{color2}"),
                            new Tuple<string, string>("clip", "noclip"),
                            new Tuple<string, string>("model", "invisible_opaque_box"),
                            new Tuple<string, string>("material", $"{outlineMaterial}"),
                            new Tuple<string, string>("no_show", "false"),
                            new Tuple<string, string>("no_decals", "true")};

                        CreateOutlineGroup(roomIndex, firstPoint, nextPoint, new Vector3(0, 0, 0), entityProperties, indexSP, indexNP, "outline", color3);
                    }
                }
            }
        }

        public List<Vector3> Thicc(Vector3 currentBlock)
        {
            List<Vector3> thiccBlocks = new List<Vector3>
            {
                // Adding blocks on X axis
                new Vector3 (currentBlock.X + 1, currentBlock.Y, currentBlock.Z),
                new Vector3 (currentBlock.X - 1, currentBlock.Y, currentBlock.Z),

                // Adding blocks on Y axis
                new Vector3 (currentBlock.X, currentBlock.Y + 1, currentBlock.Z),
                new Vector3 (currentBlock.X, currentBlock.Y - 1, currentBlock.Z),

                // Adding blocks on Z axis
                new Vector3 (currentBlock.X, currentBlock.Y, currentBlock.Z + 1),
                new Vector3 (currentBlock.X, currentBlock.Y, currentBlock.Z - 1)
            };

            return thiccBlocks;
        }

        public List<Vector3> CalculateRoomFrame(Vector3 size, Vector3 coords)
        {
            float startX    = coords.X;
            float startY    = coords.Y;
            float startZ    = coords.Z;
            float endX      = startX + size.X;
            float endY      = startY + size.Y;
            float endZ      = startZ + size.Z;

            return new List<Vector3> { new Vector3(startX, startY, startZ), new Vector3(endX, endY, endZ) };
        }

        public Dictionary<string, int> PaintBlock(Vector3 currentBlock, int roomIndex, Vector3 roomStart, Vector3 roomEnd, int roomCount, int[] matids)
        {
            Dictionary<string, int> materials = new Dictionary<string, int>();
            // Upper and lower Color3 stripes (Left, Right, Front, Back Walls)
            bool stripeUpperLowerLRFB = 
               /* currentBlock.Y == roomEnd.Y - 2 ||
                currentBlock.Y == roomStart.Y + 2 ||*/
                currentBlock.Y == roomEnd.Y - 1 ||
                currentBlock.Y == roomStart.Y + 1;

            // Left and right Color3 stripes (Left, Right, Front, Back Walls)
            bool stripeLeftRightLRFB = 
                currentBlock.X == roomStart.X       && currentBlock.Z == roomStart.Z    && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y || 
                currentBlock.X == roomEnd.X         && currentBlock.Z == roomEnd.Z      && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y ||
                currentBlock.X == roomStart.X       && currentBlock.Z == roomEnd.Z    && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y ||
                currentBlock.X == roomEnd.X         && currentBlock.Z == roomStart.Z      && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y 
                /*currentBlock.X == roomStart.X + 1   && currentBlock.Y < roomEnd.Y       && currentBlock.Y > roomStart.Y || 
                currentBlock.Z == roomStart.Z + 1   && currentBlock.Y < roomEnd.Y       && currentBlock.Y > roomStart.Y || 
                currentBlock.X == roomEnd.X - 1     && currentBlock.Y < roomEnd.Y       && currentBlock.Y > roomStart.Y || 
                currentBlock.Z == roomEnd.Z - 1     && currentBlock.Y < roomEnd.Y       && currentBlock.Y > roomStart.Y*/
                /*|| currentBlock.X == roomStart.X + 2 && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y && thicc
                || currentBlock.Z == roomStart.Z + 2 && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y && thicc
                || currentBlock.X == roomEnd.X - 2 && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y && thicc
                || currentBlock.Z == roomEnd.Z - 2 && currentBlock.Y < roomEnd.Y && currentBlock.Y > roomStart.Y && thicc*/;

            // Left and right Color3 stripes (Top, Bottom)
            bool stripeUpperLowerTBstartX = currentBlock.X == roomStart.X;// || currentBlock.X == roomStart.X + 1;
            bool stripeUpperLowerTBstartZ = currentBlock.Z == roomStart.Z;// || currentBlock.Z == roomStart.Z + 1;
            bool stripeUpperLowerTBendX = currentBlock.X == roomEnd.X;// || currentBlock.X == roomEnd.X - 1;
            bool stripeUpperLowerTBendZ = currentBlock.Z == roomEnd.Z;// || currentBlock.Z == roomEnd.Z - 1;
            bool stripeUpperLowerTBstart = (stripeUpperLowerTBstartX || stripeUpperLowerTBstartZ) && (currentBlock.Y == roomStart.Y || currentBlock.Y == roomEnd.Y);
            bool stripeUpperLowerTBend = (stripeUpperLowerTBendX || stripeUpperLowerTBendZ) && (currentBlock.Y == roomStart.Y || currentBlock.Y == roomEnd.Y);
            bool stripeUpperLowerTB = stripeUpperLowerTBstart || stripeUpperLowerTBend;


            bool stripes = stripeUpperLowerLRFB || stripeLeftRightLRFB || stripeUpperLowerTB;

            // Start/End triggers outline (Color3)
            bool triggerOutline = (currentBlock.Z == (roomStart.Z + 8) && roomIndex == 0)
                || (currentBlock.Z == (roomEnd.Z - 4) && roomIndex == roomCount - 1)
                || (currentBlock.Z == (roomStart.Z + 7) && roomIndex == 0)
                || (currentBlock.Z == (roomEnd.Z - 5) && roomIndex == roomCount - 1);

            // Start/End area (Color4)
            bool areaStartEnd = (roomIndex == 0 && currentBlock.Z < roomStart.Z + 7)
                || (roomIndex == roomCount - 1 && currentBlock.Z > roomEnd.Z - 4);

            if (areaStartEnd && !stripes)
            {
                materials.Add("left", matids[3]);
                materials.Add("right", matids[3]);
                materials.Add("front", matids[3]);
                materials.Add("back", matids[3]);
                materials.Add("top", matids[3]);
                materials.Add("bottom", matids[3]);
            }
            else
            {
                if (stripes || triggerOutline)
                {
                    materials.Add("left", matids[2]);
                    materials.Add("right", matids[2]);
                    materials.Add("front", matids[2]);
                    materials.Add("back", matids[2]);
                    materials.Add("top", matids[2]);
                    materials.Add("bottom", matids[2]);
                }
                else
                {
                    materials.Add("left", matids[0]);
                    materials.Add("right", matids[0]);
                    materials.Add("front", matids[0]);
                    materials.Add("back", matids[0]);
                    materials.Add("top", matids[1]);
                    materials.Add("bottom", matids[0]);
                }
            }
            return materials;
        }

        public void CreateSplits(int roomCount, Vector3 min, Vector3 max, List<Block> BlocksToBuild)
        {
            Vector3 minPosition = Vector3.Min(min, max);
            Vector3 maxPosition = Vector3.Max(min, max);
            int totalX = Math.Abs((int)minPosition.X) + Math.Abs((int)maxPosition.X);
            int totalZ = Math.Abs((int)minPosition.Z) + Math.Abs((int)maxPosition.Z);

            double splitCount = Math.Floor((double)roomCount / 10);
            double splitOffset, nextSplitPoint;
            bool splitAlongX = totalX >= totalZ;

            if (splitAlongX)
            {
                splitOffset = Math.Floor(totalX / splitCount);
                nextSplitPoint = minPosition.X;
            }
            else
            {
                splitOffset = Math.Floor(totalZ / splitCount);
                nextSplitPoint = minPosition.Z;
            }

            for (int i = 0; i < splitCount - 1; i++)
            {
                nextSplitPoint += splitOffset;
                float minX = maxPosition.X;
                float maxX = minPosition.X;
                float minY = maxPosition.Y;
                float maxY = minPosition.Y;
                float minZ = maxPosition.Z;
                float maxZ = minPosition.Z;

                foreach (Block block in BlocksToBuild)
                {
                    if ((splitAlongX && block.x == nextSplitPoint) || (!splitAlongX && block.z == nextSplitPoint))
                    {
                        if (splitAlongX)
                        {
                            if (block.z < minZ) minZ = block.z;
                            if (block.z > maxZ) maxZ = block.z;
                        }
                        else
                        {
                            if (block.x < minX) minX = block.x;
                            if (block.x > maxX) maxX = block.x;
                        }

                        if (block.y < minY) minY = block.y;
                        if (block.y > maxY) maxY = block.y;
                    }
                }

                int size = splitAlongX ? totalZ * 22 : totalX * 22;
                Vector3 position = splitAlongX
                    ? new Vector3((float)nextSplitPoint * 40, (minY + maxY) * 10, (maxZ + minZ) * 20 + 2)
                    : new Vector3((maxX + minX) * 20 + 2, (minY + maxY) * 10, (float)nextSplitPoint * 40);

                this.entities.Add(new Entity($"trigger_split{i}",
                    position,
                    new Vector3(0, 0, 0),
                    splitAlongX ? new Vector3(80, maxY * 8, size) : new Vector3(size, maxY * 8, 80),
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "display_split")
                    }));
            }            
        }

        public void CreateBevels(Vector3 startingPoint, Vector3 min, Vector3 max)
        {
            int totalX = Math.Abs((int)min.X) + Math.Abs((int)max.X), totalZ = Math.Abs((int)min.Z) + Math.Abs((int)max.Z);
            int csDecalsSize = totalX >= totalZ ? totalX * 60 : totalZ * 60;
            List<Tuple<string, string>> decalProperties = new List<Tuple<string, string>> {
                new Tuple<string, string>("color", "DE131313"),
                new Tuple<string, string>("material", "snow_decal_snow01"),
                new Tuple<string, string>("v3", "true")};

            Vector3 centerPosition = new Vector3((max.X + min.X) * 20, startingPoint.Y * 20 + 20, (max.Z + min.Z) * 20);

            this.entities.Add(new Entity("decal_cs_up",
                centerPosition,
                new Vector3(-90, 0, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));

            this.entities.Add(new Entity("decal_cs_down",
                centerPosition,
                new Vector3(90, 0, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));

            this.entities.Add(new Entity("decal_cs_forward",
                centerPosition,
                new Vector3(0, 0, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));

            this.entities.Add(new Entity("decal_cs_back",
                centerPosition,
                new Vector3(0, -180, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));

            this.entities.Add(new Entity("decal_cs_left",
                centerPosition,
                new Vector3(0, -90, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));

            this.entities.Add(new Entity("decal_cs_right",
                centerPosition,
                new Vector3(0, 90, 0),
                new Vector3(csDecalsSize, csDecalsSize, csDecalsSize),
                decalProperties));
        }

        public void CreateDecals(List<List<Vector3>> roomsCoordsList, bool rl)
        {
            Vector3 firstRoomA = roomsCoordsList[0][0];
            Vector3 firstRoomB = roomsCoordsList[0][1];
            if (rl)
            {
                this.entities.Add(new Entity("billboard_rl",
                   new Vector3(((int)firstRoomA.X + (int)firstRoomB.X - 4) * 20, ((int)firstRoomA.Y + (int)firstRoomB.Y - 6) * 10, ((int)firstRoomA.Z + 8) * 40),
                   new Vector3(0, 0, 0),
                   new Vector3(160, 160, 160),
                   new List<Tuple<string, string>> {
                        new Tuple<string, string>("alpha", "true"),
                        new Tuple<string, string>("backface", "true"),
                        new Tuple<string, string>("texture", "textures/decals/symbol_2.png"),
                        new Tuple<string, string>("color", "78FF00")}));
            }
            
        }

        public void CreateBFDecal(Vector3 startingPoint, Vector3 min, Vector3 max, string bfDecalPath, string bfDecalColor)
        {            
            float scaleCheck = max.X - min.X > max.Z - min.Z ? max.X - min.X : max.Z - min.Z;            
            Vector3 position = new Vector3((startingPoint.X - scaleCheck) * 40, 100, (startingPoint.Z - scaleCheck / 2) * 40);
            Vector3 rotation = new Vector3(90, 0, 0);
            Vector3 scale = new Vector3(scaleCheck * 80, scaleCheck * 80, 1);
            this.entities.Add(new Entity("billboard_bfd",
                position,
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("alpha", "true"),
                    new Tuple<string, string>("backface", "true"),
                    new Tuple<string, string>("texture", bfDecalPath),
                    new Tuple<string, string>("color", bfDecalColor)
                }));
        }

        public void CreateBillboards(List<List<Vector3>> roomsCoordsList, List<Vector3> roomsSizeList, bool startEndBillboardsFlag, string startEndBillboardsColor )
        {
            Vector3 startingPoint = roomsCoordsList[0][0];
            int numRooms          = roomsCoordsList.Count;
            int glassOffset, glassSizeOffset;            

            string color        = startEndBillboardsFlag ? $"A2{startEndBillboardsColor}" : "A2FFFFFF";
            string glassColor   = startEndBillboardsFlag ? startEndBillboardsColor : "FFFFFF";

            float startX = startingPoint.X;
            float startY = startingPoint.Y;
            float startZ = startingPoint.Z;
            float endX   = roomsCoordsList[numRooms - 1][0].X;
            float endY   = roomsCoordsList[numRooms - 1][0].Y;
            float endZ   = roomsCoordsList[numRooms - 1][1].Z;

            float startSizeX = roomsSizeList[0].X;
            float startSizeY = roomsSizeList[0].Y;
            float endSizeX   = roomsSizeList[numRooms - 1].X;
            float endSizeY   = roomsSizeList[numRooms - 1].Y;            

            glassOffset     = 0;
            glassSizeOffset = 1;                
            
            // Create the entities for the start and end triggers
            this.entities.Add(new Entity("billboard_start_trigger",
                new Vector3((startX + glassOffset) * 40, (startY + glassOffset) * 20, (startZ + 8) * 40),
                new Vector3(0, 0, 0),
                new Vector3((startSizeX + glassSizeOffset) * 40, (startSizeY + glassSizeOffset) * 20, 80),
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("alpha", "false"),
                    new Tuple<string, string>("backface", "true"),
                    new Tuple<string, string>("color", color)
                }));

            this.entities.Add(new Entity("billboard_end_trigger",
                new Vector3((endX + glassOffset) * 40, (endY + glassOffset) * 20, (endZ - 4) * 40),
                new Vector3(0, 0, 0),
                new Vector3((endSizeX + glassSizeOffset) * 40, (endSizeY + glassSizeOffset) * 20, 80),
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("alpha", "false"),
                    new Tuple<string, string>("backface", "true"),
                    new Tuple<string, string>("color", color)
                        }));
            this.entities.Add(new Entity("billboard_start_glass",
                new Vector3((startX + glassOffset) * 40, (startY + glassOffset) * 20, (startZ + 8) * 40),
                new Vector3(0, 0, 0),
                new Vector3((startSizeX + glassSizeOffset) * 40, (startSizeY + glassSizeOffset) * 20, 80),
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("alpha", "false"),
                    new Tuple<string, string>("backface", "true"),
                    new Tuple<string, string>("texture", "billboard_glass"),
                    new Tuple<string, string>("color", glassColor)
                }));

            this.entities.Add(new Entity("billboard_end_glass",
                new Vector3((endX + glassOffset) * 40, (endY + glassOffset) * 20, (endZ - 4) * 40),
                new Vector3(0, 0, 0),
                new Vector3((endSizeX + glassSizeOffset) * 40, (endSizeY + glassSizeOffset) * 20, 80),
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("alpha", "false"),
                    new Tuple<string, string>("backface", "true"),
                    new Tuple<string, string>("texture", "billboard_glass"),
                    new Tuple<string, string>("color", glassColor)
                }));
        }

        public void CreateLogicChatCommands(List<List<Vector3>> roomsCoordsList)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            // !discord
            this.entities.Add(new Entity("logic_discord_cc",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "cc_discord"),
                    new Tuple<string, string>("trigger", "on_chat_command"),
                    new Tuple<string, string>("chat_command", "discord"),
                    new Tuple<string, string>("chat_command_description", "URL for DBT Racing Discord"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_discord",
                new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "https://discord.gg/Bf4ztWHR"),
                    new Tuple<string, string>("logic_group", "cc_discord"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));

            // !lbs
            this.entities.Add(new Entity("logic_lbs_cc",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "cc_lbs"),
                    new Tuple<string, string>("trigger", "on_chat_command"),
                    new Tuple<string, string>("chat_command", "lbs"),
                    new Tuple<string, string>("chat_command_description", "URL for DBT Leaderboards website"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_lbs",
                new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "timetrials.org/leaderboards"),
                    new Tuple<string, string>("logic_group", "cc_lbs"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));

            // !reset
            this.entities.Add(new Entity("logic_reset_cc",
                new Vector3((startingPoint.X + endPoint.X + 12) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "cc_reset"),
                    new Tuple<string, string>("trigger", "on_chat_command"),
                    new Tuple<string, string>("chat_command", "reset"),
                    new Tuple<string, string>("chat_command_description", "Reset score and splits"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_reset",
                new Vector3((startingPoint.X + endPoint.X + 14) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "reset_splits"),
                    new Tuple<string, string>("logic_group", "cc_reset"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_reset_text",
                new Vector3((startingPoint.X + endPoint.X + 16) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "Reset completed"),
                    new Tuple<string, string>("logic_group", "cc_lbs"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));

            // !fix
            this.entities.Add(new Entity("logic_fix_cc",
                new Vector3((startingPoint.X + endPoint.X + 20) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "cc_fix"),
                    new Tuple<string, string>("trigger", "on_chat_command"),
                    new Tuple<string, string>("chat_command", "fix"),
                    new Tuple<string, string>("chat_command_description", "Fix physics on the map (if any presented, e.g. dj, haste)"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
                
            this.entities.Add(new Entity("logic_fix_phy_logic_group",
                new Vector3((startingPoint.X + endPoint.X + 22) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                    new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "cc_fix"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
                
                
            this.entities.Add(new Entity("logic_fix_text",
                new Vector3((startingPoint.X + endPoint.X + 26) * 20, (startingPoint.Y + 57) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "Fix completed"),
                    new Tuple<string, string>("logic_group", "cc_fix"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));
            
            /// 
            /// logic (!race)
            /// 
            this.entities.Add(new Entity("logic_race_cc",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 30) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("chat_command", "race"),
                    new Tuple<string, string>("chat_command_description", "Changes physics to Race"),
                    new Tuple<string, string>("group_to_trigger", "OCC_race"),
                    new Tuple<string, string>("player_targets", "triggering_player"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("trigger", "on_chat_command")}));
            this.entities.Add(new Entity("logic_race_set_phy",
                new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 30) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_player_physics"),
                    new Tuple<string, string>("logic_group", "OCC_race"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("physics", "race"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_race_text",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 30) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "display_text"),
                    new Tuple<string, string>("logic_group", "OCC_race"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("text_to_display", "Race Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_race_phy_logic_group",
                new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 30) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "OCC_race"),
                    new Tuple<string, string>("logic_group_order", "3"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_race_chat",
                new Vector3((startingPoint.X + endPoint.X + 10) * 20, (startingPoint.Y + 30) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("logic_group", "OCC_race"),
                    new Tuple<string, string>("logic_group_order", "4"),
                    new Tuple<string, string>("message_text", "Race Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            /// 
            /// logic (!vintage)
            ///
            this.entities.Add(new Entity("logic_vintage_cc",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 33) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("chat_command", "vintage"),
                    new Tuple<string, string>("chat_command_description", "Changes physics to Vintage"),
                    new Tuple<string, string>("group_to_trigger", "OCC_vintage"),
                    new Tuple<string, string>("player_targets", "triggering_player"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("trigger", "on_chat_command")}));
            this.entities.Add(new Entity("logic_vintage_set_phy",
                new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 33) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_player_physics"),
                    new Tuple<string, string>("logic_group", "OCC_vintage"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("physics", "vintage"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_vintage_text",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 33) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "display_text"),
                    new Tuple<string, string>("logic_group", "OCC_vintage"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("text_to_display", "Vintage Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_vintage_phy_logic_group",
                new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 33) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "OCC_vintage"),
                    new Tuple<string, string>("logic_group_order", "3"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
            this.entities.Add(new Entity("logic_vintage_chat",
                new Vector3((startingPoint.X + endPoint.X + 10) * 20, (startingPoint.Y + 33) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("logic_group", "OCC_vintage"),
                    new Tuple<string, string>("logic_group_order", "4"),
                    new Tuple<string, string>("message_text", "Vintage Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));

            /// 
            /// logic (!diabotical)
            ///
            this.entities.Add(new Entity("logic_diabotical_cc",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 36) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("chat_command", "diabotical"),
                    new Tuple<string, string>("chat_command_description", "Changes physics to Diabotical Vanilla"),
                    new Tuple<string, string>("group_to_trigger", "OCC_diabotical"),
                    new Tuple<string, string>("player_targets", "triggering_player"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("trigger", "on_chat_command")}));

            this.entities.Add(new Entity("logic_diabotical_set_phy",
                new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 36) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_player_physics"),
                    new Tuple<string, string>("logic_group", "OCC_diabotical"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("physics", "diabotical"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));

            this.entities.Add(new Entity("logic_diabotical_text",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 36) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "display_text"),
                    new Tuple<string, string>("logic_group", "OCC_diabotical"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("text_to_display", "diabotical Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));

            this.entities.Add(new Entity("logic_diabotical_phy_logic_group",
                new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 36) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "OCC_diabotical"),
                    new Tuple<string, string>("logic_group_order", "3"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));

            this.entities.Add(new Entity("logic_diabotical_chat",
                new Vector3((startingPoint.X + endPoint.X + 10) * 20, (startingPoint.Y + 36) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("logic_group", "OCC_diabotical"),
                    new Tuple<string, string>("logic_group_order", "4"),
                    new Tuple<string, string>("message_text", "diabotical Physics"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player")}));
        }

        public void CreateLogicOnGameLaunch(List<List<Vector3>> roomsCoordsList)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            this.entities.Add(new Entity("logic_on_game_start_lbs_text",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 54) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "Visit Time Trials leaderboards:"),
                    new Tuple<string, string>("delay", "600000"),
                    new Tuple<string, string>("number_of_loops", "10"),
                    new Tuple<string, string>("trigger", "on_game_start"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_on_game_start_lbs_url",
                new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + 54) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "timetrials.org/leaderboards"),
                    new Tuple<string, string>("delay", "600001"),
                    new Tuple<string, string>("number_of_loops", "10"),
                    new Tuple<string, string>("trigger", "on_game_start"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
        }

        public void CreateLogicOnRestart(List<List<Vector3>> roomsCoordsList, bool haste, bool rl)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            this.entities.Add(new Entity("logic_on_restart",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "on_restart"),
                    new Tuple<string, string>("trigger", "on_player_spawn"),
                    new Tuple<string, string>("delay", "50"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
            this.entities.Add(new Entity("logic_on_restart_match_started",
                new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "on_restart"),
                    new Tuple<string, string>("trigger", "on_game_start"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));
            this.entities.Add(new Entity("logic_on_restart_phy",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "on_restart"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));

            if (haste)
            {
                this.entities.Add(new Entity("logic_on_restart_r_powerups",
                    new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "remove_powerups"),
                        new Tuple<string, string>("logic_group", "on_restart"),
                        new Tuple<string, string>("logic_group_order", "1"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
                this.entities.Add(new Entity("logic_on_restart_airaccel",
                    new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "set_game_variable"),
                        new Tuple<string, string>("variable_name", "phy_accel_air"),
                        new Tuple<string, string>("variable_value", "1"),
                        new Tuple<string, string>("logic_group", "on_restart"),
                        new Tuple<string, string>("logic_group_order", "2"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
            }

            if (rl)
            {

                this.entities.Add(new Entity("logic_on_restart_r_weapon",
                    new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 51) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "remove_weapon"),
                        new Tuple<string, string>("weapon", "rocket_launcher"),
                        new Tuple<string, string>("logic_group", "on_restart"),
                        new Tuple<string, string>("logic_group_order", "3"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
            }

        }

        public void CreateLogicOnStart(List<List<Vector3>> roomsCoordsList, bool rl, bool haste)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            this.entities.Add(new Entity("logic_on_start",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 48) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "on_start"),
                    new Tuple<string, string>("entered_entity", "trigger_start"),
                    new Tuple<string, string>("trigger", "on_entity_entered"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            if (rl)
            {
                this.entities.Add(new Entity("logic_on_start_rl",
                    new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 48) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "give_weapon"),
                        new Tuple<string, string>("ammo", "999"),
                        new Tuple<string, string>("autoswitch", "true"),
                        new Tuple<string, string>("weapon", "rocket_launcher"),
                        new Tuple<string, string>("logic_group", "on_start"),
                        new Tuple<string, string>("logic_group_order", "1"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
            }

            if (haste)
            {
                this.entities.Add(new Entity("logic_on_start_haste",
                    new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 48) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "trigger_logic_group"),
                        new Tuple<string, string>("group_to_trigger", "haste"),
                        new Tuple<string, string>("logic_group", "on_start"),
                        new Tuple<string, string>("logic_group_order", "2"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
            }
        }

        public void CreateLogicOnConnect(List<List<Vector3>> roomsCoordsList)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            this.entities.Add(new Entity("logic_on_connect",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 45) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "on_connect"),
                    new Tuple<string, string>("trigger", "on_player_connect"),
                    new Tuple<string, string>("delay", "6000"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_on_connect_phy",
                new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 45) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "trigger_logic_group"),
                    new Tuple<string, string>("group_to_trigger", "phy"),
                    new Tuple<string, string>("logic_group", "on_connect"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
            

            this.entities.Add(new Entity("logic_on_connect_welcome",
                new Vector3((startingPoint.X + endPoint.X + 14) * 20, (startingPoint.Y + 45) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "Welcome to Time Trials! Join our community: diabotical-racing.com"),
                    new Tuple<string, string>("logic_group", "on_connect"),
                    new Tuple<string, string>("logic_group_order", "3"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));

            this.entities.Add(new Entity("logic_on_connect_help",
                new Vector3((startingPoint.X + endPoint.X + 16) * 20, (startingPoint.Y + 45) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "send_server_message"),
                    new Tuple<string, string>("message_text", "Type !help for custom commands list (in console)"),
                    new Tuple<string, string>("logic_group", "on_connect"),
                    new Tuple<string, string>("logic_group_order", "4"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "all_players"),}));
        }

        public void CreateLogicHaste(List<List<Vector3>> roomsCoordsList)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            this.entities.Add(new Entity("logic_haste_accel",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 42) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "haste"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("variable_name", "phy_accel_air"),
                    new Tuple<string, string>("variable_value", "2.5"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

            this.entities.Add(new Entity("logic_haste_powerup",
                new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + 42) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "give_haste"),
                    new Tuple<string, string>("logic_group", "haste"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("powerup_duration", "999999"),
                    new Tuple<string, string>("powerup_announce", "false"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets ", "triggering_player"),}));
            
        }
        
        public void CreateLogicPhysics(List<List<Vector3>> roomsCoordsList, bool upramps, string gravity, bool dj, bool mj)
        {            
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            if (upramps)
            {
                this.entities.Add(new Entity("logic_upramps_imp_up",
                    new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "phy"),
                    new Tuple<string, string>("logic_group_order", "1"),
                    new Tuple<string, string>("variable_name", "phy_ramp_rel_impulse_up"),
                    new Tuple<string, string>("variable_value", "4"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

                this.entities.Add(new Entity("logic_upramps_speedm",
                    new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "phy"),
                    new Tuple<string, string>("logic_group_order", "2"),
                    new Tuple<string, string>("variable_name", "phy_ramp_speed_multiplier"),
                    new Tuple<string, string>("variable_value", "0.8"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));

                this.entities.Add(new Entity("logic_upramps_speed",
                    new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "phy"),
                    new Tuple<string, string>("logic_group_order", "3"),
                    new Tuple<string, string>("variable_name", "phy_ramp_up_speed"),
                    new Tuple<string, string>("variable_value", "0.1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
            }
            this.entities.Add(new Entity("logic_gravity",
                new Vector3((startingPoint.X + endPoint.X + 6) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "phy"),
                    new Tuple<string, string>("logic_group_order", "4"),
                    new Tuple<string, string>("variable_name", "phy_gravity"),
                    new Tuple<string, string>("variable_value", gravity),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
            if (dj)
            {
                this.entities.Add(new Entity("logic_dj",
                new Vector3((startingPoint.X + endPoint.X + 8) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("action", "set_game_variable"),
                    new Tuple<string, string>("logic_group", "phy"),
                    new Tuple<string, string>("logic_group_order", "5"),
                    new Tuple<string, string>("variable_name", "phy_double_jump"),
                    new Tuple<string, string>("variable_value", "1"),
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("player_targets", "triggering_player"),}));
            }
            if (mj)
            {
                this.entities.Add(new Entity("logic_mj_triple_jump",
                    new Vector3((startingPoint.X + endPoint.X + 10) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "set_game_variable"),
                        new Tuple<string, string>("logic_group", "phy"),
                        new Tuple<string, string>("logic_group_order", "6"),
                        new Tuple<string, string>("variable_name", "phy_triple_jump"),
                        new Tuple<string, string>("variable_value", "255"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
                this.entities.Add(new Entity("logic_mj_step_up",
                    new Vector3((startingPoint.X + endPoint.X + 12) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "set_game_variable"),
                        new Tuple<string, string>("logic_group", "phy"),
                        new Tuple<string, string>("logic_group_order", "7"),
                        new Tuple<string, string>("variable_name", "phy_step_up"),
                        new Tuple<string, string>("variable_value", "21"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
                this.entities.Add(new Entity("logic_mj_multi_jump",
                    new Vector3((startingPoint.X + endPoint.X + 14) * 20, (startingPoint.Y + 39) * 20, (startingPoint.Z - 10) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>> {
                        new Tuple<string, string>("action", "set_game_variable"),
                        new Tuple<string, string>("logic_group", "phy"),
                        new Tuple<string, string>("logic_group_order", "8"),
                        new Tuple<string, string>("variable_name", "phy_multi_jump"),
                        new Tuple<string, string>("variable_value", "100"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("player_targets", "triggering_player"),}));
            }

        }

        public void CreateEntitiesLights(List<List<Vector3>> roomsCoordsList, bool shadowAmbient, string shadowAmbientColor, bool fog, string fogColor, bool ambient, string ambientColor, bool sun, string sunColor)
        {            
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            if (shadowAmbient)
            {
                this.entities.Add(new Entity("light_shadow_ambient",
                    new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 5) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>>{
                        new Tuple<string, string>("type", "shadow_ambient"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("color", shadowAmbientColor)}));
            }

            if (fog)
            {
                this.entities.Add(new Entity("light_fog",
                    new Vector3((startingPoint.X + endPoint.X + 1) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 5) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>>{
                        new Tuple<string, string>("type", "fog"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("intensity", "8"),
                        new Tuple<string, string>("radius", "-1"),
                        new Tuple<string, string>("margin", "0"),
                        new Tuple<string, string>("radius", "15000"),
                        new Tuple<string, string>("color", fogColor)}));
            }
            
            if (ambient)
            {
                this.entities.Add(new Entity("light_ambient",
                    new Vector3((startingPoint.X + endPoint.X + 2) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 5) * 40),
                    rotation,
                    scale,
                    new List<Tuple<string, string>>{
                        new Tuple<string, string>("ambient", "true"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("type", "ambient"),
                        new Tuple<string, string>("radius", "2000"),
                        new Tuple<string, string>("color", ambientColor)}));
            }
            
            if (sun)
            {
                this.entities.Add(new Entity("light_sun",
                    new Vector3((startingPoint.X + endPoint.X + 3) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 5) * 40),
                    new Vector3(59, -23, 0),
                    new Vector3(1, 1, 1),
                    new List<Tuple<string, string>>{
                        new Tuple<string, string>("sun", "1"),
                        new Tuple<string, string>("unique", "1"),
                        new Tuple<string, string>("type", "sun"),
                        new Tuple<string, string>("intensity", "4"),
                        new Tuple<string, string>("color", sunColor)}));
            }
            
        }

        public void CreateEntitiesBasics(List<List<Vector3>> roomsCoordsList, List<Vector3> roomsSizeList, bool skyboxFlag, string skyboxName, string accent1)
        {
            Vector3 startingPoint   = roomsCoordsList[0][0];
            Vector3 endPoint        = roomsCoordsList[0][1];
            Vector3 rotation        = new Vector3(0, 0, 0);
            Vector3 scale           = new Vector3(1, 1, 1);
            skyboxName = skyboxFlag ? skyboxName : "black.png";

            this.entities.Add(new Entity("spawn",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + 4) * 20, (startingPoint.Z + 5) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>>()));
            CreateSmartSpawns(startingPoint, endPoint, accent1);

            this.entities.Add(new Entity("global",
                new Vector3((startingPoint.X + endPoint.X + 4) * 20, (startingPoint.Y + 4) * 20, (startingPoint.Z + 5) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>> {
                    new Tuple<string, string>("unique", "1"),
                    new Tuple<string, string>("bevel", "5"),
                    new Tuple<string, string>("accent1", accent1),
                    new Tuple<string, string>("skybox", skyboxName),
                    }));

            this.entities.Add(new Entity("trigger_start",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 9) * 40),
                rotation,
                new Vector3(roomsSizeList[0].X * 42, roomsSizeList[0].Y * 20, 50),
                new List<Tuple<string, string>> { new Tuple<string, string>("action", "start_timer") }));

            this.entities.Add(new Entity("trigger_end",
                new Vector3((roomsCoordsList[roomsCoordsList.Count - 1][0].X + roomsCoordsList[roomsCoordsList.Count - 1][1].X + 1) * 20, (roomsCoordsList[roomsCoordsList.Count - 1][0].Y + roomsCoordsList[roomsCoordsList.Count - 1][1].Y) * 10, (roomsCoordsList[roomsCoordsList.Count - 1][1].Z - 3) * 40),
                rotation,
                new Vector3(roomsSizeList[roomsSizeList.Count - 1].X * 42, roomsSizeList[roomsSizeList.Count - 1].Y * 20, 50),
                new List<Tuple<string, string>> { new Tuple<string, string>("action", "end_timer") }));

            this.entities.Add(new Entity("thumbnail",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 2) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>>()));
            this.entities.Add(new Entity("camera_intro_1_start",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 2) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>>()));

            this.entities.Add(new Entity("camera_intro_1_end",
                new Vector3((startingPoint.X + endPoint.X) * 20, (startingPoint.Y + endPoint.Y) * 10, (startingPoint.Z + 50) * 40),
                rotation,
                scale,
                new List<Tuple<string, string>>()));
        }

        public void CreateSmartSpawns(Vector3 startingPoint, Vector3 endPoint, string accent1)
        {
            Vector3 rotation    = new Vector3(0, 0, 0);
            Vector3 scale       = new Vector3(1, 1, 1);
            int i = 1;
            float abs   = endPoint.X - startingPoint.X;
            float delta = (abs - 2) / 8;
            for (float z = startingPoint.Z + 1.25f; z <= startingPoint.Z + 8; z += 3)
            {
                for (float x = startingPoint.X + 1.35f; x <= endPoint.X; x += delta)
                {
                    // spawn entity
                    this.entities.Add(new Entity($"spawn_{i}",
                        new Vector3((x) * 40, (startingPoint.Y + 3) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>>{
                            new Tuple<string, string> ("spawn_group", $"spawn_{i}")
                        }));
                    // trigger zone
                    this.entities.Add(new Entity($"trigger_spawn_{i}",
                        new Vector3((x) * 40, (startingPoint.Y + 0.5f) * 20, (z) * 40),
                        rotation,
                        new Vector3((delta - 0.64f) * 40, 20, 93),
                        new List<Tuple<string, string>>()));

                    // logic to switch spawns
                    this.entities.Add(new Entity($"logic_spawn_{i}",
                        new Vector3((x) * 40, (startingPoint.Y + 30) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "trigger_logic_group"),
                            new Tuple<string, string>("allow_multiple_teams", "true"),
                            new Tuple<string, string>("capture_time", "3000"),
                            new Tuple<string, string>("entity_to_capture", $"trigger_spawn_{i}"),
                            new Tuple<string, string>("forbidden_tags", $"spawn_{i}"),
                            new Tuple<string, string>("group_to_trigger", $"spawn_{i}"),
                            new Tuple<string, string>("maximum_captures", "0"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("trigger", "on_area_captured"),
                            new Tuple<string, string>("unique", "1")
                        }));

                    this.entities.Add(new Entity($"logic_spawn_{i}_set_spawns",
                        new Vector3((x) * 40, (startingPoint.Y + 33) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "set_player_spawns"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("spawn_groups", $"spawn_{i}"),
                            new Tuple<string, string>("spawn_preset", "groups"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "0"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    this.entities.Add(new Entity($"logic_spawn_{i}_clear_tags",
                        new Vector3((x) * 40, (startingPoint.Y + 34) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "clear_player_tags"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "1"),
                            new Tuple<string, string>("unique", "1")
                        }));  
                    this.entities.Add(new Entity($"logic_spawn_{i}_message",
                        new Vector3((x) * 40, (startingPoint.Y + 35) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "display_text"),
                            new Tuple<string, string>("text_to_display", "Spawnpoint changed"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "2"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    this.entities.Add(new Entity($"logic_spawn_{i}_ping_player",
                        new Vector3((x) * 40, (startingPoint.Y + 36) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "ping_player"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("team_ping", "false"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "3"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    this.entities.Add(new Entity($"logic_spawn_{i}_light_off",
                        new Vector3((x) * 40, (startingPoint.Y + 37) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "disable_light"),
                            new Tuple<string, string>("delay", "3500"),
                            new Tuple<string, string>("light_entity", $"light_spawn_{i}"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "4"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    this.entities.Add(new Entity($"logic_spawn_{i}_add_tag",
                        new Vector3((x) * 40, (startingPoint.Y + 38) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "add_player_tag"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("tag_to_add", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group", $"spawn_{i}"),
                            new Tuple<string, string>("logic_group_order", "5"),
                            new Tuple<string, string>("unique", "1")
                        }));

                    this.entities.Add(new Entity($"logic_spawn_{i}_light_1sec",
                        new Vector3((x) * 40, (startingPoint.Y + 40) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "modify_light"),
                            new Tuple<string, string>("allow_multiple_teams", "true"),
                            new Tuple<string, string>("capture_time", "999"),
                            new Tuple<string, string>("entity_to_capture", $"trigger_spawn_{i}"),
                            new Tuple<string, string>("group_to_trigger", $"spawn_{i}"),
                            new Tuple<string, string>("maximum_captures", "0"),
                            new Tuple<string, string>("action", "modify_light"),
                            new Tuple<string, string>("light_color", accent1),
                            new Tuple<string, string>("light_entity", $"light_spawn_{i}"),
                            new Tuple<string, string>("light_intensity", "15"),
                            new Tuple<string, string>("light_radius", "500"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("trigger", "on_area_captured"),
                            new Tuple<string, string>("forbidden_tags", $"spawn_{i}"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    this.entities.Add(new Entity($"logic_spawn_{i}_lights_off_enter",
                        new Vector3((x) * 40, (startingPoint.Y + 41) * 20, (z) * 40),
                        rotation,
                        scale,
                        new List<Tuple<string, string>> {
                            new Tuple<string, string>("action", "disable_light"),
                            new Tuple<string, string>("trigger", "on_entity_entered"),
                            new Tuple<string, string>("player_targets", "triggering_player"),
                            new Tuple<string, string>("delay", "4500"),
                            new Tuple<string, string>("entered_entity", $"trigger_spawn_{i}"),
                            new Tuple<string, string>("light_entity", $"light_spawn_{i}"),
                            new Tuple<string, string>("unique", "1")
                        }));
                    


                    // spawn zone lights
                    Vector3 position = new Vector3((x) * 40, (startingPoint.Y + 1) * 20, (z) * 40);
                    float radius = 10;
                    List<Tuple<string, string>> properties = new List<Tuple<string, string>> {
                        new Tuple<string, string>("color", "FFFFFF"),
                        new Tuple<string, string>("intensity", "1"),
                        new Tuple<string, string>("type", "diffuse"),
                        new Tuple<string, string>("radius", $"{radius}"),};
                    this.entities.Add(new Entity($"light_spawn_{i}", position, rotation, scale, properties));

                    i++;
                }
            }
        }

        public void CreateArrows(int i, int direction, Vector3 prevRoomStart, Vector3 prevRoomEnd, string routeArrowsColor)
        {
            Vector3 rotation = direction == 0 ? new Vector3(0, 0, 270) : new Vector3(0, 0, 90);
            Vector3 scale = new Vector3(90, 90, 1);
            int groupCounter = 0;
            List<Tuple<string, string>> properties = new List<Tuple<string, string>> {
                        new Tuple<string, string>("alpha", "true"),
                        new Tuple<string, string>("color", routeArrowsColor),
                        new Tuple<string, string>("backface", "true"),
                        new Tuple<string, string>("fade_distance_max", "300"),
                        new Tuple<string, string>("fade_distance_min", "1400"),
                        new Tuple<string, string>("group", $"{i}"),
                        new Tuple<string, string>("group_member_id", $"{groupCounter}"),
                        new Tuple<string, string>("backface", "true"),
                        new Tuple<string, string>("texture", "textures/core/items/coin_stack_bg_arrow"),
                        };

            Vector3 position = new Vector3((prevRoomStart.X + prevRoomEnd.X) * 20 - 60, (prevRoomStart.Y + prevRoomEnd.Y) * 10, (prevRoomStart.Z + prevRoomEnd.Z) * 20);
            this.entities.Add(new Entity($"billboard_room_{i}_right_{groupCounter}", position, rotation, scale, properties));

            groupCounter++;
            properties[6] = new Tuple<string, string>("group_member_id", $"{groupCounter}");
            position = new Vector3((prevRoomStart.X + prevRoomEnd.X) * 20, (prevRoomStart.Y + prevRoomEnd.Y) * 10, (prevRoomStart.Z + prevRoomEnd.Z) * 20);
            this.entities.Add(new Entity($"billboard_room_{i}_right_{groupCounter}", position, rotation, scale, properties));

            groupCounter++;
            properties[6] = new Tuple<string, string>("group_member_id", $"{groupCounter}");
            position = new Vector3((prevRoomStart.X + prevRoomEnd.X) * 20 + 60, (prevRoomStart.Y + prevRoomEnd.Y) * 10, (prevRoomStart.Z + prevRoomEnd.Z) * 20);
            this.entities.Add(new Entity($"billboard_room_{i}_right_{groupCounter}", position, rotation, scale, properties));
        }

        public List<List<Vector3>> RoomsCoordsList(int roomCount, Vector3 startingPoint, List<Vector3> roomsSizeList, bool roomRandomApproach, bool routeArrowsFlag, string routeArrowsColor)
        {
            List<List<Vector3>> roomsCoordsList = new List<List<Vector3>>();
            Random random = new Random();
            int buildingHeight;
            float roomHeight = roomsSizeList[0].Y;

            float nextRoomStartX = startingPoint.X;
            float nextRoomStartY = startingPoint.Y;
            float nextRoomStartZ = startingPoint.Z;            

            for (int i = 0; i < roomCount; i++)
            {
                buildingHeight = random.Next(3);
                float currentWidth = roomsSizeList[i].X;
                float currentDepth = roomsSizeList[i].Z;
                int buildingDirection, direction = 1;

                if (i > 0)
                {
                    float prevWidth = roomsSizeList[i - 1].X;
                    float prevDepth = roomsSizeList[i - 1].Z;                    
                    Vector3 prevRoomStart   = roomsCoordsList[i - 1][0];
                    Vector3 prevRoomEnd     = roomsCoordsList[i - 1][1];

                    if (i == 1 || i == roomCount - 1)
                    {
                        buildingDirection = 1; // Forward
                    }
                    else
                    {
                        buildingDirection = roomRandomApproach ? random.Next(3) : (i % 2 == 0 ? 1 : random.Next(3));
                    }

                    // Calculate X and Z points
                    if (buildingDirection == 0) // Right
                    {
                        nextRoomStartX = prevRoomEnd.X;
                        nextRoomStartZ = prevDepth > currentDepth ? 
                            random.Next((int)prevRoomStart.Z, (int)(prevRoomEnd.Z - currentDepth)) : 
                            random.Next((int)(prevRoomEnd.Z - currentDepth), (int)prevRoomStart.Z) ;
                        direction = buildingDirection;
                    }
                    if (buildingDirection == 1) // Forward
                    {                        
                        nextRoomStartX = prevWidth > currentWidth ? 
                            random.Next((int)prevRoomStart.X, (int)(prevRoomEnd.X - currentWidth)) : 
                            random.Next((int)(prevRoomEnd.X - currentWidth), (int)prevRoomStart.X) ;
                        nextRoomStartZ = (int)prevRoomEnd.Z;
                    }
                    if (buildingDirection == 2) // Left
                    {
                        nextRoomStartX = (int)prevRoomStart.X - currentWidth;
                        nextRoomStartZ = prevDepth > currentDepth ? 
                            random.Next((int)prevRoomStart.Z, (int)(prevRoomEnd.Z - currentDepth)) : 
                            random.Next((int)(prevRoomEnd.Z - currentDepth), (int)prevRoomStart.Z) ;
                        direction = buildingDirection;
                    }
                    // Calculate Y point
                    if (buildingHeight == 0) // Same height
                    {
                        nextRoomStartY = (int)prevRoomStart.Y;
                    }
                    if (buildingHeight == 1) // Upper
                    {
                        nextRoomStartY = random.Next((int)prevRoomStart.Y, (int)(prevRoomStart.Y + (roomHeight / 3)));
                    }
                    if (buildingHeight == 2) // Lower
                    {
                        nextRoomStartY = random.Next((int)(prevRoomStart.Y - (roomHeight / 3)), (int)prevRoomStart.Y);
                    }

                    // billboards arrows

                    if (routeArrowsFlag && (direction == 0 || direction == 2))
                    {
                        CreateArrows(i, direction, prevRoomStart, prevRoomEnd, routeArrowsColor);
                    }
                } 
                else
                {
                    nextRoomStartX = startingPoint.X;
                    nextRoomStartY = startingPoint.Y;
                    nextRoomStartZ = startingPoint.Z;                    
                }                   

                // Calculate room frames (starting XYZ, ending XYZ)
                List<Vector3> lastRoomCoords = CalculateRoomFrame(roomsSizeList[i], new Vector3(nextRoomStartX, nextRoomStartY, nextRoomStartZ));
                roomsCoordsList.Add(lastRoomCoords);                

                // Update previous dimensions
                roomHeight = roomsSizeList[i].Y;                
            }

            return roomsCoordsList;
        }

        public List<Vector3> RoomsSizeList (int roomCount, double roomSizeMultiply, Random random)
        {
            List<Vector3> roomsSizeList = new List<Vector3>();

            // Generating size of the rooms
            for (int i = 0; i < roomCount; i++)
            {
                bool isFirstRoom = i == 0 ? true : false;
                Vector3 size = GenerateRooms(random, roomSizeMultiply, isFirstRoom);
                roomsSizeList.Add(size);
            }

            return roomsSizeList;
        }
    
    }
}