// See https://aka.ms/new-console-template for more information
using Corps.Server.Services;
using MegaCorps.Core.Model;

ImageService imageService = new ImageService(
    "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Direction\\directions.json",
    "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Description",
    "..\\..\\..\\..\\Corps.Server\\Resource\\Image"
    );
Console.WriteLine("Trying to read the resource files...");
Console.WriteLine("DIRECTIONS:");
imageService.directions.ForEach(direction => { Console.WriteLine(string.Join(" ", new List<string>() { direction.Title, direction.Direction.ToString() })); });
Console.WriteLine("INFOS:");
imageService.attackInfos.ForEach(info =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { info.Title, info.Description, info.AttackType.ToString() }));
});
imageService.defenceInfos.ForEach(info =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { info.Title, info.Description, Convert.ToString(info.AttackTypeList?.Count )??""}));
});
imageService.developerInfos.ToList().ForEach(info =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { info.Title, info.Description}));
});
Console.WriteLine("IMAGES:");
imageService.BackgroundImages.ForEach(image =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { image.Name, image.Type.ToString() }));
});
imageService.cardBackgroundImages.ForEach(image =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { image.Name, image.Type.ToString() }));
});
imageService.cardIconImages.ForEach(image =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { image.Name, image.Type.ToString() }));
});
imageService.UserImages.ForEach(image =>
{
    Console.WriteLine(string.Join(" ", new List<string>() { image.Name, image.Type.ToString() }));
});

GameEngine gameEngine = new GameEngine(4);

List<CardDTO> dTOs = await imageService.GetCardDTOs(gameEngine.Deck.UnplayedCards);

dTOs.ForEach(cardDTO => {
    Console.WriteLine(string.Join(" ",
        new List<string>() {
            Convert.ToString(cardDTO.Id),
            cardDTO.Type,
            string.Join("|",new List<string> { 
                cardDTO.Info.Title, 
                cardDTO.Info.Description, 
                cardDTO.Info.Direction??"", 
                Convert.ToString(cardDTO.IconImageId),
                Convert.ToString(cardDTO.BackgroundImageId),
                Convert.ToString(cardDTO.Info.Power)??"" 
            }),
            Convert.ToString(cardDTO.Background.Length),
            Convert.ToString(cardDTO.Icon.Length) }));
});
