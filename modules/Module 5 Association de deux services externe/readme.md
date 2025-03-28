# Module 5 Association de deux services externe

Démarrer avec le projet du module précédent:

```
git clone https://github.com/CroquetMickael/RestDotNetIntegrationTests.git --branch feature/module4
```

## Préambule

Ce module sera légérement plus en autonomie, nous allons ajouter un nouveau service.

Notre client n'aime pas trop devoir rentrer une latitude / longitude pour trouver sa météo de la semaine, de ce fait, nous allons appeller un service avec une adresse et il nous renverra ces informations.

Cette API étant payante, pour des raisons de budget dans l'entreprise, nous aimerions que vous utilisiez ce que vous avez appris pour mocker le tout.

## Le schema de la nouvelle API

Voici le schema `Open API` de notre API GEOCODE:

<details>
<summary>OPENAPI SCHEMA GEO CODE</summary>
<br>

```yml
openapi: 3.0.2
info:
  title: Geocoding Api
  version: 1.0.0
  description: Geocoding Api Documentation
  contact:
    url: https://openapi.it/en/support
    name: Support
  termsOfService: https://openapi.it/en/terms-and-conditions
  license:
    name: Apache 2.0
    url: http://www.apache.org/licenses/LICENSE-2.0.html
paths:
  /geocode:
    post:
      tags:
        - geocode
      operationId: geocode
      summary: Retrieve informations about a place supplying address
      requestBody:
        description: "To improve success of results, please specify an address conforming to the following format: [street], [city] [postal code] [country]  "
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/Address"
      responses:
        "200":
          description: Retrieve place element
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Place"
  /reverse:
    post:
      tags:
        - reverse
      operationId: reverse
      summary: Get place information from ID or latitude/longitude
      requestBody:
        description: 'Get place information from ID or latitude/longitude: <br><ul><li>To obtain infos via ID make sure to pass the following format: <pre>{"type": "id", "id": "&lt;id&gt;"}</pre></li><li>To obtain infos via lat/long, make sure to provide the following format: <pre>{"type": "coordinates", "lat": "&lt;lat&gt;", "long": "&lt;long&gt;"}</pre></li></ul>'
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/ReverseModel"
      responses:
        "200":
          description: Get place element on response
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Place"
security:
  - bearerAuth: []
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
  schemas:
    Token:
      type: object
      properties:
        token:
          type: string
          readOnly: true
    Address:
      type: object
      properties:
        address:
          type: string
          example: Via Cristoforo Colombo, Roma RM
      required:
        - address
    ReverseModel:
      type: object
      properties:
        id:
          type: string
          example: v442c6653ee93a5733e8a9ea2f842cd5c250d2b6ab
        type:
          type: string
          example: id | coordinates
        lat:
          type: number
          format: float
          example: 41.289294
        long:
          type: number
          format: float
          example: 13.2349029
      required:
        - type
    Place:
      type: object
      properties:
        success:
          type: string
          example: "true"
        elements:
          type: object
          properties:
            id:
              type: string
              example: v442c6653ee93a5733e8a9ea2f842cd5c250d2b6ab
            element:
              type: object
              properties:
                providedBy:
                  type: string
                  example: google_maps | openstreetmap
                latitude:
                  type: number
                  format: float
                  example: 41.808916
                longitude:
                  type: number
                  format: float
                  example: 12.4535602
                bounds:
                  type: object
                  properties:
                    south:
                      type: number
                      format: float
                      example: 41.7691104
                    west:
                      type: number
                      format: float
                      example: 12.3831487
                    north:
                      type: number
                      format: float
                      example: 41.8731993
                    east:
                      type: number
                      format: float
                      example: 12.4985637
                streetNumber:
                  type: string
                  example: "null"
                streetName:
                  type: string
                  example: Via Cristoforo Colombo
                postalCode:
                  type: string
                  example: "04019"
                locality:
                  type: string
                  example: Roma
                subLocality:
                  type: string
                  example: "null"
                adminLevels:
                  type: object
                  properties:
                    "1":
                      type: object
                      properties:
                        name:
                          type: string
                          example: Lazio
                        code:
                          type: string
                          example: Lazio
                        level:
                          type: integer
                          example: 1
                    "2":
                      type: object
                      properties:
                        name:
                          type: string
                          example: Citta Metropolitana di Roma
                        code:
                          type: string
                          example: RM
                        level:
                          type: integer
                          example: 2
                    "3":
                      type: object
                      properties:
                        name:
                          type: string
                          example: Roma
                        code:
                          type: string
                          example: Roma
                        level:
                          type: integer
                          example: 3
                country:
                  type: string
                  example: Italy
                countryCode:
                  type: string
                  example: IT
                timezone:
                  type: string
                  example: "null"
servers:
  - url: https://geocoding.openapi.it
    description: Production
  - url: https://test.geocoding.openapi.it
    description: Sandbox
externalDocs:
  description: First time here? Generate a new access token
  url: https://console.openapi.com/oauth
```

</details>

## Information technique

Quand vous allez ajouter votre service pensez bien à rajouter l'option suivante en plus des précédente: `/GenerateExceptionClasses:false`.

### Pourquoi rajouter /GenerateExceptionClasses:false ?

Ayant déjà rajouter un service, [NSWAG](https://github.com/RicoSuter/NSwag) l'outil qui s'occupe de la génération de notre client aura déjà généré les exceptions de base qu'il utilise pour le service `OpenMeteo` de ce fait, il y aura 2 fois `ApiExecption` dans la solution.

Ce qui va entrainer de l'ambiguité pour le SDK .net et il ne buildera plus la solution.

L'option permet simplement de ne pas générer à nouveau les classes d'exception par défaut de `NSWAG` et ainsi ne pas créer cette ambiguité.

## WithMainArtifact / WithSecondaryArtifact

Les deux sous fonction de la création du container `Microcks` prennent plusieurs paramètres en entrée :

```cs
.WithMainArtifacts("openapi2.yml", "monAutreDefinitionDeService.yml")
.WithSecondaryArtifacts("Mocks\\OpenMeteo\\openMeteoMocks.yml","monAutreMockDeMaDefinitionDeService.yml")
```

## Pour résumer

Nous voulons garder la route API créer dans le module 1, mais nous voulons que l'utilisateur renseigne une adresse qui appellera l'API "GeoCode" qui renverra alors une Latitude / Longitude lié à votre adresse que vous allez réutiliser pour rappeller le premier service que vous avez intégré au début "OpenMeteo"

/!\ Pensez à bien réutiliser la même Longitude / Latitude dans vos mocks !
