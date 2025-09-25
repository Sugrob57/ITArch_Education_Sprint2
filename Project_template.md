## Изучите [README.md](README.md) файл и структуру проекта.

# Задание 1

С4 Диаграмма контейнеров текущего, промежуточного решения 
![c4.container.asis](/docs/diagrams/images/c4.container.asis.png)

С4 Диаграмма контейнеров целевого решения 
![c4.container.tobe](/docs/diagrams/images/c4.container.tobe.png)

# Задание 2

## 1. Proxy

[proxy service (kong) configuration](/src/microservices/proxy/kong.yml) - реализован на базе "Kong".

Постепенный переход, можно регулировать изменив значения movies-upstream в [kong.yml](/src/microservices/proxy/kong.yml)

### 2. Kafka
 [MVP сервис events](/src/microservices/events/KafkaConsumerService.cs), который будет при вызове API создавать и сам же читать сообщения в топике Kafka.

Cкриншот тестов
![Cкриншот тестов](/docs/screens/postman.tests.t3.png) 

Cкриншот состояния топиков Kafka http://localhost:8090 
![Cкриншот состояния топиков Kafka](/docs/screens/kafka.t3.png) 


# Задание 3

### CI/CD

- В файле [docker-build-push.yml](/.github/workflows/docker-build-push.yml) добавлены шаги сборки events-service и movies-service
  - proxy-service не добавлен, т.к. сборка для него не требуется - используется публичный образ "kong:3.8"
- api-tests и сборка включены для ветки "cinema", успешно проходят:
  - [сборка](https://github.com/Sugrob57/ITArch_Education_Sprint2/actions/runs/17923515915)
  - [тесты](https://github.com/Sugrob57/ITArch_Education_Sprint2/actions/runs/17923515913)

### Proxy в Kubernetes

- Доработаны файлы [event-service.yaml](/src/kubernetes/events-service.yaml) и [proxy-service.yaml](/src/kubernetes/proxy-service.yaml)
- Добавлен файл [kong-config-configmap.yaml](/src/kubernetes/kong-config-configmap.yaml) для настройки proxy-service

Лог event-service при прогоне тестов, во время обработки событий:
![Cкриншот обработки событий](/docs/screens/kubernetes.event-service.logs.png) 

# Задание 4

- отредактирован файл [values.yaml](/src/kubernetes/helm/values.yaml)
- отредактирован файл [ingress.yaml](/src/kubernetes/helm/templates/ingress.yaml)
- подготовлен шаблон [events-service.yaml](/src/kubernetes/helm/templates/services/events-service.yaml)
- созданы файлы шаблона для proxy-service 
  - [proxy-service/deployment.yaml](/src/kubernetes/helm/templates/proxy-service/deployment.yaml)
  - [proxy-service/service.yaml](/src/kubernetes/helm/templates/proxy-service/service.yaml)
  - [proxy-service/configmap.yaml](/src/kubernetes/helm/templates/proxy-service/configmap.yaml)

Cкриншот развертывания helm:
![Cкриншот развертывания helm](/docs/screens/helm.release.staus.cinemaabyss.png) 

Cкриншот вывода https://cinemaabyss.example.com/api/movies :
![Cкриншот вывода api/movies](/docs/screens/helm.api.movies.response.png) 

# Задание 5
Компания планирует активно развиваться и для повышения надежности, безопасности, реализации сетевых паттернов типа Circuit Breaker и канареечного деплоя вам как архитектору необходимо развернуть istio и настроить circuit breaker для monolith и movies сервисов.

1. Подготовлены DestinationRule и VirtualService - в файле [circuit-breaker-config.yaml](/src/kubernetes/circuit-breaker-config.yaml) есть комментарии
2. Proxy-service переключен на "100% трафика в monolith", т.к. movies-service может быть доступен через ingress напрямую.

Вывод работы circuit breaker'а:
![circuit-breaker-config.yaml](/docs/screens/fortio.isitio-test.1.png)
