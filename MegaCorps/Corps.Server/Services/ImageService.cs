using Corps.Server.DTO;
using Corps.Server.Hubs;
using Corps.Server.Utils.JSON;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Common;
using MegaCorps.Core.Model.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Corps.Server.Services
{
    public class ImageService
    {
        private List<Image> backgroundImages = new List<Image>();
        private List<Image> userImages = new List<Image>();
        public List<Image> BackgroundImages { get => backgroundImages; set => backgroundImages = value; }
        public List<Image> UserImages { get => userImages; set => userImages = value; }

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
            using (StreamReader r = new StreamReader(directionPath))
            {
                string json = r.ReadToEnd();
                directions = DataSerializer.Deserialize<List<CardDirectionInfo>>(json)!;
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\attack_descriptions.json"))
            {
                string json = r.ReadToEnd();
                attackInfos = DataSerializer.Deserialize<List<AttackCardDescriptionInfo>>(json)!;
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\defence_descriptions.json"))
            {
                string json = r.ReadToEnd();
                defenceInfos = DataSerializer.Deserialize<List<DefenceCardDescriptionInfo>>(json)!;
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\developer_descriptions.json"))
            {
                string json = r.ReadToEnd();
                List<DeveloperCardDescriptionInfo> infos = DataSerializer.Deserialize<List<DeveloperCardDescriptionInfo>>(json)!;
                infos.Sort(new CardDescriptionComparer());
                developerInfos = new Queue<DeveloperCardDescriptionInfo>(infos);
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\event_descriptions.json"))
            {
                string json = r.ReadToEnd();
                eventInfos = new Queue<EventCardDescriptionInfo>(DataSerializer.Deserialize<List<EventCardDescriptionInfo>>(json)!);
            }
        }

        private void GetImageData(string folderPath)
        {
            BackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Background\\Board", ImageType.Board));
            BackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Background\\Menu", ImageType.Menu));
            UserImages.AddRange(GetImagesFromFolder(folderPath + "\\UserIcon", ImageType.UserIcon));
            cardBackgroundImages.AddRange(GetImagesFromFolder(folderPath + "\\Card\\Background", ImageType.CardBackground));
            cardIconImages.AddRange(GetImagesFromFolder(folderPath + "\\Card\\Icon", ImageType.CardIcon));
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
        public async Task SendImagesToClients(List<Image> images, IClientProxy group)
        {
            await group.SendAsync("ReceiveImages", images.Select(bg => bg.ImageData).ToList());
        }

        public List<CardDirectionInfo> directions = new List<CardDirectionInfo>();
        public List<AttackCardDescriptionInfo> attackInfos = new List<AttackCardDescriptionInfo>();
        public List<DefenceCardDescriptionInfo> defenceInfos = new List<DefenceCardDescriptionInfo>();
        public Queue<DeveloperCardDescriptionInfo> developerInfos = new Queue<DeveloperCardDescriptionInfo>();
        public List<Image> cardBackgroundImages = new List<Image>();
        public List<Image> cardIconImages = new List<Image>();
        public Queue<EventCardDescriptionInfo> eventInfos = new Queue<EventCardDescriptionInfo>();

        public async Task<List<CardDTO>> GetCardDTOs(List<GameCard> cards)
        {
            Image attackBackground = cardBackgroundImages.Where(x => x.Name.Contains("attack")).First();
            Image defenceBackground = cardBackgroundImages.Where(x => x.Name.Contains("defence")).First();
            Image developerBackground = cardBackgroundImages.Where(x => x.Name.Contains("developer")).First();
            Image eventBackground = cardBackgroundImages.Where(x => x.Name.Contains("event")).First();
            List<CardDTO> DTO = new List<CardDTO>();
            cards.ForEach(x =>
            {
                if (x is AttackCard)
                {
                    AttackCardDescriptionInfo info = attackInfos.Where(y => y.AttackType == (x as AttackCard)!.AttackType).First();

                    DTO.Add(
                    new CardDTO()
                    {
                        Id = x.Id,
                        Type = "attack",
                        BackgroundImageId = attackBackground.Id,
                        Background = attackBackground.ImageData,
                        IconImageId = info.Id,
                        Icon = cardIconImages.Find(x => x.Id == info.Id)?.ImageData ?? "",
                        Info = new CardInfoDTO()
                        {
                            Title = info.Title,
                            Description = info.Description,
                            Direction = directions.Where(y => y.Direction == (x as AttackCard)!.Direction).First().Title,
                            Power = (x as AttackCard)!.Damage,
                        },
                    });
                }
                else if (x is DefenceCard)
                {
                    DefenceCardDescriptionInfo info =
                    defenceInfos.Where(
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
                        Icon = cardIconImages.Find(x => x.Id == info.Id)?.ImageData ?? "",
                        Info = new CardInfoDTO()
                        {
                            Title = info.Title,
                            Description = info.Description,
                            AttackTypes = string.Join("---",
                                        attackInfos.Select(x => x.AttackType)
                                                    .Where(y => info.AttackTypeList!.Contains(y))
                                                    .Select(z => attackInfos.Find(r => r.AttackType == z)!.AttackTypeName))
                        },
                    });
                }
                else if (x is DeveloperCard)
                {
                    DeveloperCardDescriptionInfo developerInfo = developerInfos.Dequeue();
                    developerInfos.Enqueue(developerInfo);
                    DTO.Add(
                    new CardDTO()
                    {
                        Id = x.Id,
                        Type = "developer",
                        BackgroundImageId = developerBackground.Id,
                        Background = developerBackground.ImageData,
                        Icon = cardIconImages.Find(x => x.Id == developerInfo.Id)?.ImageData ?? "",
                        IconImageId = developerInfo.Id,
                        Info = new CardInfoDTO()
                        {
                            Title = developerInfo.Title,
                            Description = developerInfo.Description,
                            Power = (x as DeveloperCard)!.DevelopmentPoint
                        },
                    });
                }
                else if(x is EventCard)
                {
                    EventCardDescriptionInfo eventInfo = eventInfos.Dequeue();
                    int power = 0;

                    if (x is ScoreEventCard) { power = (x as ScoreEventCard)!.Power; }
                    if(x is NeighboursEventCards) { power = (x as NeighboursEventCards)!.Power; }
                    if (x is AllLosingCard) { power = (x as AllLosingCard)!.Power; }

                    eventInfos.Enqueue(eventInfo);
                    DTO.Add(
                        new CardDTO()
                        {
                            Id = x.Id,
                            Type = "event",
                            BackgroundImageId = eventBackground.Id,
                            Background = eventBackground.ImageData,
                            Icon = cardIconImages.Find(x => x.Id == eventInfo.Id)?.ImageData ?? "",
                            IconImageId = eventInfo.Id,
                            Info = new CardInfoDTO()
                            {
                                Title = eventInfo.Title,
                                Description = eventInfo.Description,
                                Power = power > 0? power:null,
                            }
                        });
                }
            });
            return DTO;
        }

    }
}
