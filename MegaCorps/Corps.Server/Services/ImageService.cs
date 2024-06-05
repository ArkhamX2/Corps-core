using Corps.Server.Hubs;
using Corps.Server.Utils.JSON;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
public class Image
{
    public string Name { get; set; }
    public ImageType Type { get; set; }
    public string ImageData { get; set; }
}

public enum ImageType
{
    Menu,
    Board,
    CardBackground,
    CardIcon,
    UserIcon,
}

public class CardDTO
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Background { get; set; }
    public string Icon { get; set; }
    public CardInfoDTO Info { get; set; }
}

public class CardInfoDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Direction { get; set; }
    public int? Power { get; set; }
}

public class AttackCardDescriptionInfo
{
    [JsonProperty("attack_type")]
    public AttackType AttackType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}
public class DefenceCardDescriptionInfo
{
    [JsonProperty("attack_types")]
    public List<AttackTypeDTO> AttackTypeList { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class AttackTypeDTO
{
    [JsonProperty("attack_type")]
    public AttackType AttackType { get; set; }
}
public class DeveloperCardDescriptionInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class CardDirectionInfo
{
    [JsonProperty("card_direction")]
    public CardDirection Direction { get; set; }
    public string Title { get; set; }
}

public class ImageComparer : IComparer<Image>
{
    public int Compare(Image x, Image y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        return x.Name.CompareTo(y.Name);
    }
}
public class AttackTypeComparer : IEqualityComparer<AttackType>
{
    public bool Equals(AttackType x, AttackType y)
    {
        return x == y;
    }

    public int GetHashCode(AttackType obj)
    {
        return obj.ToString().GetHashCode() ^ obj.ToString().GetHashCode();
    }
}

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
            directionPath=DirectionPath;
            descriptionPath=DescriptionPath; 
            imagePath=ImagePath;
            GetTextData(directionPath, descriptionPath);
            GetImageData(imagePath);
        }


        private void GetTextData(string directionPath, string descriptionPath)
        {
            using (StreamReader r = new StreamReader(directionPath))
            {
                string json = r.ReadToEnd();
                directions = DataSerializer.Deserialize<List<CardDirectionInfo>>(json);
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\attack_descriptions.json"))
            {
                string json = r.ReadToEnd();
                attackInfos = DataSerializer.Deserialize<List<AttackCardDescriptionInfo>>(json);
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\defence_descriptions.json"))
            {
                string json = r.ReadToEnd();
                defenceInfos = DataSerializer.Deserialize<List<DefenceCardDescriptionInfo>>(json);
            }
            using (StreamReader r = new StreamReader(descriptionPath + "\\developer_descriptions.json"))
            {
                string json = r.ReadToEnd();
                developerInfos = new Queue<DeveloperCardDescriptionInfo>(DataSerializer.Deserialize<List<DeveloperCardDescriptionInfo>>(json));
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
        public List<CardDTO> GetCardDTOs(List<GameCard> cards)
        {
            byte[] attackBackgroundImageData = cardBackgroundImages.Where(x => x.Name.Contains("attack")).First().ImageData;
            byte[] defenceBackgroundImageData = cardBackgroundImages.Where(x => x.Name.Contains("defence")).First().ImageData;
            byte[] developerBackgroundImageData = cardBackgroundImages.Where(x => x.Name.Contains("developer")).First().ImageData;
            Queue<byte[]> developerIconImageDataQueue = new Queue<byte[]>(cardIconImages.Where(x => x.Name.Contains("developer")).ToList().OrderBy(x => x, new ImageComparer()).Select(x => x.ImageData));
            List<CardDTO> DTO = new List<CardDTO>();
            cards.ForEach(x =>
            {
                if (x is AttackCard)
                {
                    DTO.Add(
                    new CardDTO()
                    {
                        Id = x.Id,
                        Type = "attack",
                        Background = attackBackgroundImageData,
                        Icon = cardIconImages.Where(x => x.Name.Contains("attack")).First().ImageData,
                        Info = new CardInfoDTO()
                        {
                            Title = attackInfos.Where(y => y.AttackType == (x as AttackCard).AttackType).First().Title,
                            Description = attackInfos.Where(y => y.AttackType == (x as AttackCard).AttackType).First().Description,
                            Direction = directions.Where(y => y.Direction == (x as AttackCard).Direction).First().Title,
                            Power = (x as AttackCard).Damage,
                        },
                    });
                }
                else if (x is DefenceCard)
                {
                    //TODO: Отправляет список типов атак от которых защищает
                    DefenceCardDescriptionInfo defenceCardDescriptionInfo = 
                    defenceInfos.Where(
                        y => y.AttackTypeList.Select(z => z.AttackType).
                        SequenceEqual((x as DefenceCard).AttackTypes, new AttackTypeComparer())
                        ).First();

                    DTO.Add(
                    new CardDTO()
                    {
                        Id = x.Id,
                        Type = "defence",
                        Background = defenceBackgroundImageData,
                        Icon = cardIconImages.Where(x => x.Name.Contains("defence")).First().ImageData,
                        Info = new CardInfoDTO()
                        {
                            Title = defenceCardDescriptionInfo.Title,
                            Description = defenceCardDescriptionInfo.Description,
                            
                        },
                    });
                }
                else if (x is DeveloperCard)
                {
                    DeveloperCardDescriptionInfo developerInfo = developerInfos.Dequeue();
                    byte[] developerIconImageData = developerIconImageDataQueue.Dequeue();
                    developerInfos.Enqueue(developerInfo);
                    developerIconImageDataQueue.Enqueue(developerIconImageData);
                    DTO.Add(
                    new CardDTO()
                    {
                        Id = x.Id,
                        Type = "developer",
                        Background = developerBackgroundImageData,
                        Icon = developerIconImageData,
                        Info = new CardInfoDTO()
                        {
                            Title = developerInfo.Title,
                            Description = developerInfo.Description,
                        },
                    });
                }
            });
            return DTO;
        }

    }
}
