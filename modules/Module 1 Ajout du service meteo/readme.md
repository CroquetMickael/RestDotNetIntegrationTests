# Module 1 : création du projet de test

## Qu'allons-nous faire ?

Nous allons ajouter une référence vers un service externe nommé [Open Meteo](https://open-meteo.com/en/docs), qui sera, par la suite, mocké dans les tests.

## Instructions

Récupérez le projet en executant la commande suivante :

```
git clone https://github.com/CroquetMickael/RestDotNetIntegrationTests.git --branch feature/init
```

Ouvrez la solution, puis ajoutez une référence de service connecté. La capture d'écran suivante est un exemple d'ajout de service sur Visual Studio 2022.

![ConnectedService](img/ConnectedService.png)

Après avoir double-cliqué sur "Connected Services", rendez vous dans la partie "Services References" et cliquez sur l'icône du "+" en haut à droite de l'encart concerné.

![Comment Ajouter un service](img/ConnectedServiceOpenAPI.png)

Plusieurs choix vous sont proposés. Dans le cadre de notre projet, nous allons choisir "OpenAPI".

![Choix du type de service](img/ConnectedServiceAddOpenAPI.png)

L'éditeur vous demandera un ensemble d'informations pour ajouter le service que nous souhaitons connecter à notre solution. Voici les informations à renseigner :

- URL : https://raw.githubusercontent.com/open-meteo/open-meteo/refs/heads/main/openapi.yml
- Espace de noms du code généré : MyApi.WebApi.Services
- Nom de la classe pour le code généré : OpenMeteoApi
- Langage de génération du code : C#
- Options de génération du code supplémentaire : /GenerateBaseUrlProperty:false /useBaseUrl:false

![Informations du service](img/ConnectedServiceFinalAddOpenAPI.png)

Un fichier `.yml` va être généré avec le nom fourni, ici OpenMeteoApi.
Votre fichier généré sera donc `OpenMeteoApi.yml`

Lorsque nous renseignons les propriétés `/GenerateBaseUrlProperty:false /useBaseUrl:false` dans la configuration de génération de la classe du **service connecté**, nous désactivons le paramétrage automatique de l'URL dans la classe générée. Cela nous permet de travailler librement et de définir une URL personnalisée et simulée.

## Utilisation du nouveau service

Dans le projet MyApi.WebApi, créer un dossier `Services` et ajoutez les 2 classes et l'interface suivantes :

- OpenMeteoService.cs
- MeteoServiceObject.cs
- IOpenMeteoService.cs

Dans le fichier **OpenMeteoService**, nous allons modifier le constructeur pour injecter l'objet généré par les Services Connectés via l'injection de dépendance et permettre l'implémentation de l'interface que vous venez de créer.
Nous allons ajouter deux méthodes : 

- la première sera publique et permettra d'effectuer l'appel au service REST via l'objet injecté
- la deuxième sera responsable du mapping des données.

```cs
using MyApi.WebApi.Services;

namespace MyApi.WebApi.Services;

public class OpenMeteoService : IOpenMeteoService
{
    protected readonly OpenMeteoApi _openMeteo;

    public OpenMeteoService(OpenMeteoApi openMeteo)
    {
        _openMeteo = openMeteo;
    }

    public async Task<MeteoServiceObject> GetMeteo(
        double latitude,
        double longitude)
    {
        var response = await _openMeteo.ForecastAsync([], [Anonymous2.Temperature_2m_max, Anonymous2.Temperature_2m_min], latitude, longitude, false, Temperature_unit.Celsius, null, null, "GMT", null);
        return MapOpenMeteoApiResponse(response);
    }

    private MeteoServiceObject MapOpenMeteoApiResponse(Response meteoApiResponse)
    {
        var meteoService = new MeteoServiceObject();
        var WeatherDataByDay = new List<Temperature>();
        meteoService.Latitude = meteoApiResponse.Latitude;
        meteoService.Longitude = meteoApiResponse.Longitude;
        DailyResponse dailyResponse = meteoApiResponse.Daily;
        for (int i = 0; i < dailyResponse.Time.Count; i++)
        {
            Temperature temperature = new Temperature
            {
                Min = $"{dailyResponse.Temperature_2m_min.ToArray()[i]}",
                Max = $"{dailyResponse.Temperature_2m_max.ToArray()[i]}",
                Date = dailyResponse.Time.ToArray()[i]
            };
            WeatherDataByDay.Add(temperature);
        }
        meteoService.Temperature_By_Times = WeatherDataByDay;

        return meteoService;
    }
}

public class WeatherData
{
    public List<string> Time { get; set; }
    public List<double> Temperature_2m_max { get; set; }
    public List<double> Temperature_2m_min { get; set; }
}
```

Ici, nous avons appelé le service via l'objet généré avec des données par défaut, comme l'utilisation des degrés en Celsius, et de mapper les données à l'aide d'une classe dédiée.

Vous remarquerez l'utilisation d'un type nommé `MeteoServiceObject` qui n'est pas présent dans cette classe. Nous allons donc ajouter cette classe dans le même dossier.

```cs
namespace MyApi.WebApi.Services;

public class MeteoServiceObject
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public IEnumerable<Temperature> Temperature_By_Times  {get; set;}
}

public class Temperature
{
    public string Date { get; set; }

    public string Min { get; set; }

    public string Max { get; set; }
}
```

Nous allons maintenant modifier l'interface `IOpenMeteoService`, contrat de notre service.

```cs
namespace MyApi.WebApi.Services;

public interface IOpenMeteoService
{
    Task<MeteoServiceObject> GetMeteo(
        double latitude,
        double longitude);
}
```

## Configuration de l'injection de dépendance

Dans le Program.cs, nous allons ajouter l'injection des dépendances de notre service.

Tout d'abord, nous allons ajouter les références nécessaires pour l'injection de notre service par son interface, puis la configuration de l'HTTPClient pour exploiter le service REST.

```cs
builder.Services.AddTransient<IOpenMeteoService, OpenMeteoService>();
builder.Services.AddHttpClient<OpenMeteoApi>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com");
});
```

Nous configurons l'URL de base de notre HttpClient en dur dans le code. Lorsque nous serons à l'étape de test, nous ajouterons notre URL personnalisée.

## Modification des controlleurs existants

Nous avons besoin d'agrémenter le constructeur de la classe de notre controlleur afin d'injecter le service.

```cs
private readonly IOpenMeteoService _openMeteoApi;

public WeatherForecastController(IOpenMeteoService openMeteoApi)
{
    _openMeteoApi = openMeteoApi;
}
```

### Ajout du modèle de donnée de la requête

Il nous faut définir un modèle spécifique pour le `QueryParam` que nous allons ajouter dans notre controlleur.

Créez un dossier `Model` et ajoutez-y cette classe :

```cs
namespace MyApi.WebApi.Model;

public class MeteoObject
{
    public double Latitude { get; set; } = 52.2f;
    public double Longitude { get; set; } = 69.9f;
}
```

Une fois toutes ces étapes réalisées, nous pouvons ajouter notre point d'entrée qui se chargera de l'appel du service.

```cs
[HttpGet]
[Route("SevenDayMinMax")]
public async Task<MeteoServiceObject?> Get([FromQuery] MeteoObject meteo)
{
    return await _openMeteoApi.GetMeteo(meteo.Latitude, meteo.Longitude);
}
```

## Test via le Swagger

Lancez le projet. La configuration par défaut vous ammenera sur la page du Swagger :

![Swagger](img/Swagger.png)

Cliquez sur le bouton "Try it out", puis renseignez les valeurs de `latitude` et de `longitude`. Vous pouvez y inscrire les valeurs `double` que vous souhaitez.

Voici un exemple de réponse validant l'appel du service :

```json
{
  "latitude": 14,
  "longitude": 52,
  "temperature_By_Times": [
    {
      "date": "2025-01-27",
      "min": "22,9",
      "max": "24,2"
    },
    {
      "date": "2025-01-28",
      "min": "23,4",
      "max": "24,2"
    },
    {
      "date": "2025-01-29",
      "min": "23,7",
      "max": "24,4"
    },
    {
      "date": "2025-01-30",
      "min": "24",
      "max": "24,4"
    },
    {
      "date": "2025-01-31",
      "min": "24",
      "max": "24,7"
    },
    {
      "date": "2025-02-01",
      "min": "23",
      "max": "24,5"
    },
    {
      "date": "2025-02-02",
      "min": "23",
      "max": "23,9"
    }
  ]
}
```

```
git clone https://github.com/CroquetMickael/RestDotNetIntegrationTests.git --branch feature/module1
```

[suivant >](../Module%202%20Ajout%20des%20tests%20du%20service%20externe/readme.md)
