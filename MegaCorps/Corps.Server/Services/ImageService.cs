using Corps.Server.DTO;
using Corps.Server.Utils.JSON;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Common;

namespace Corps.Server.Services
{
    public class ImageService
    {
        public List<Image> BackgroundImages { get; set; } = [];
        public List<Image> UserImages { get; set; } = [];
        public List<CardDirectionInfo> Directions { get; set; } = [];
        public List<AttackCardDescriptionInfo> AttackInfos { get; set; } = [];
        public List<DefenceCardDescriptionInfo> DefenceInfos { get; set; } = [];
        public Queue<DeveloperCardDescriptionInfo> DeveloperInfos { get; set; } = [];
        public List<Image> CardBackgroundImages { get; set; } = [];
        public List<Image> CardIconImages { get; set; } = [];
        public Queue<EventCardDescriptionInfo> EventInfos { get; set; } = [];

        public string directionPath = ".\\Resource\\Text\\Card\\Direction\\directions.json";
        public string descriptionPath = ".\\Resource\\Text\\Card\\Description";
        public string imagePath = ".\\Resource\\Image";


        public ImageService()
        {
            GetTextData(directionPath, descriptionPath);
            GetImageData(imagePath);
        }
        public ImageService(string DirectionPath, string DescriptionPath, string ImagePath)
        {
            directionPath = DirectionPath;
            descriptionPath = DescriptionPath;
            imagePath = ImagePath;
            GetTextData(directionPath, descriptionPath);
            GetImageData(imagePath);
        }
        private void GetTextData(string directionPath, string descriptionPath)
        {
            GetDirectionsData(directionPath);
            GetAttackDescriptions(descriptionPath);
            GetDefenceDescriptions(descriptionPath);
            GetDeveloperDescriptions(descriptionPath);
            GetEventDescriptions(descriptionPath);
        }

        private void GetEventDescriptions(string descriptionPath)
        {
            using (StreamReader r = new StreamReader(descriptionPath + "\\event_descriptions.json"))
            {
                string json = r.ReadToEnd();
                EventInfos = new Queue<EventCardDescriptionInfo>(DataSerializer.Deserialize<List<EventCardDescriptionInfo>>(json)!);
            }
        }

        private void GetDeveloperDescriptions(string descriptionPath)
        {
            using (StreamReader r = new StreamReader(descriptionPath + "\\developer_descriptions.json"))
            {
                string json = r.ReadToEnd();
                List<DeveloperCardDescriptionInfo> infos = DataSerializer.Deserialize<List<DeveloperCardDescriptionInfo>>(json)!;
                infos.Sort(new CardDescriptionComparer());
                DeveloperInfos = new Queue<DeveloperCardDescriptionInfo>(infos);
            }
        }

        private void GetDefenceDescriptions(string descriptionPath)
        {
            using (StreamReader r = new StreamReader(descriptionPath + "\\defence_descriptions.json"))
            {
                string json = r.ReadToEnd();
                DefenceInfos = DataSerializer.Deserialize<List<DefenceCardDescriptionInfo>>(json)!;
            }
        }

        private void GetAttackDescriptions(string descriptionPath)
        {
            using (StreamReader r = new StreamReader(descriptionPath + "\\attack_descriptions.json"))
            {
                string json = r.ReadToEnd();
                AttackInfos = DataSerializer.Deserialize<List<AttackCardDescriptionInfo>>(json)!;
            }
        }

        private void GetDirectionsData(string directionPath)
        {
            using (StreamReader r = new StreamReader(directionPath))
            {
                string json = r.ReadToEnd();
                Directions = DataSerializer.Deserialize<List<CardDirectionInfo>>(json)!;
            }
        }

        private void GetImageData(string folderPath)
        {
            BackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Background\\Board", ImageType.Board));
            BackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Background\\Menu", ImageType.Menu));
            UserImages.AddRange(GetImagesFromFolder(folderPath + "\\UserIcon", ImageType.UserIcon));
            CardBackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Card\\Background", ImageType.CardBackground));
            CardIconImages.AddRange(GetImagesFromFolder(folderPath + "\\Card\\Icon", ImageType.CardIcon));
        }

        private List<Image> GetImagesFromFolder(string folderPath, ImageType type)
        {
            if (Directory.Exists(folderPath))
            {
                return Directory.GetFiles(folderPath, "*.png").ToList().Select(imagePath => new Image
                {
                    Id = int.Parse(imagePath.Split('\\').Last().Split('.').First().Split('_').First()),
                    Name = imagePath.Split('\\').Last().Split('.').First(),
                    ImageData = Convert.ToBase64String(File.ReadAllBytes(imagePath)),
                    Type = type
                }).ToList();
            }
            throw new Exception("Указанный путь не найден");
        }
        public async Task<List<CardDTO>> GetCardDTOs(List<GameCard> cards)
        {
            Image attackBackground = CardBackgroundImages.Where(x => x.Name.Contains("attack")).First();
            Image defenceBackground = CardBackgroundImages.Where(x => x.Name.Contains("defence")).First();
            Image developerBackground = CardBackgroundImages.Where(x => x.Name.Contains("developer")).First();
            Image eventBackground = CardBackgroundImages.Where(x => x.Name.Contains("event")).First();
            List<CardDTO> DTO = new List<CardDTO>();
            cards.ForEach(x =>
            {
                if (x is AttackCard)
                {
                    AddAttackCard(x, attackBackground, DTO);
                }
                else if (x is DefenceCard)
                {
                    AddDefenceCard(x, defenceBackground, DTO);
                }
                else if (x is DeveloperCard)
                {
                    AddDeveloperCard(x, developerBackground, DTO);
                }
                else if (x is EventCard)
                {
                    AddEventCard(x, eventBackground, DTO);
                }
            });
            return DTO;
        }

        private void AddEventCard(GameCard x, Image eventBackground, List<CardDTO> DTO)
        {
            EventCardDescriptionInfo eventInfo = EventInfos.Dequeue();
            int power = 0;

            if (x is ScoreEventCard) { power = (x as ScoreEventCard)!.Power; }
            if (x is NeighboursEventCards) { power = (x as NeighboursEventCards)!.Power; }
            if (x is AllLosingCard) { power = (x as AllLosingCard)!.Power; }

            EventInfos.Enqueue(eventInfo);
            DTO.Add(
                new CardDTO()
                {
                    Id = x.Id,
                    Type = "event",
                    BackgroundImageId = eventBackground.Id,
                    Background = eventBackground.ImageData,
                    Icon = CardIconImages.Find(x => x.Id == eventInfo.Id)?.ImageData ?? "",
                    IconImageId = eventInfo.Id,
                    Info = new CardInfoDTO()
                    {
                        Title = eventInfo.Title,
                        Description = eventInfo.Description,
                        Power = power > 0 ? power : null,
                    }
                });
        }

        private void AddDeveloperCard(GameCard x, Image developerBackground, List<CardDTO> DTO)
        {
            DeveloperCardDescriptionInfo developerInfo = DeveloperInfos.Dequeue();
            DeveloperInfos.Enqueue(developerInfo);
            DTO.Add(
            new CardDTO()
            {
                Id = x.Id,
                Type = "developer",
                BackgroundImageId = developerBackground.Id,
                Background = developerBackground.ImageData,
                Icon = CardIconImages.Find(x => x.Id == developerInfo.Id)?.ImageData ?? "",
                IconImageId = developerInfo.Id,
                Info = new CardInfoDTO()
                {
                    Title = developerInfo.Title,
                    Description = developerInfo.Description,
                    Power = (x as DeveloperCard)!.DevelopmentPoint
                },
            });
        }

        private void AddDefenceCard(GameCard x, Image defenceBackground, List<CardDTO> DTO)
        {
            DefenceCardDescriptionInfo info =
                                DefenceInfos.Where(
                                    y => y.AttackTypeList.Select(z => z).
                                    SequenceEqual((x as DefenceCard)!.AttackTypes, new AttackTypeComparer())
                                    ).First();

            DTO.Add(
            new CardDTO()
            {
                Id = x.Id,
                Type = "defence",
                BackgroundImageId = defenceBackground.Id,
                Background = defenceBackground.ImageData,
                IconImageId = info.Id,
                Icon = CardIconImages.Find(x => x.Id == info.Id)?.ImageData ?? "",
                Info = new CardInfoDTO()
                {
                    Title = info.Title,
                    Description = info.Description,
                    AttackTypes = string.Join("---",
                                AttackInfos.Select(x => x.AttackType)
                                            .Where(y => info.AttackTypeList!.Contains(y))
                                            .Select(z => AttackInfos.Find(r => r.AttackType == z)!.AttackTypeName))
                },
            });
        }

        private void AddAttackCard(GameCard x, Image attackBackground, List<CardDTO> DTO)
        {
            AttackCardDescriptionInfo info = AttackInfos.Where(y => y.AttackType == (x as AttackCard)!.AttackType).First();

            DTO.Add(
            new CardDTO()
            {
                Id = x.Id,
                Type = "attack",
                BackgroundImageId = attackBackground.Id,
                Background = attackBackground.ImageData,
                IconImageId = info.Id,
                Icon = CardIconImages.Find(x => x.Id == info.Id)?.ImageData ?? "",
                Info = new CardInfoDTO()
                {
                    Title = info.Title,
                    Description = info.Description,
                    Direction = Directions.Where(y => y.Direction == (x as AttackCard)!.Direction).First().Title,
                    Power = (x as AttackCard)!.Damage,
                },
            });
        }
    }
}
