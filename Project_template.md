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

```bash

helm repo add istio https://istio-release.storage.googleapis.com/charts
helm repo update

helm install istio-base istio/base -n istio-system --set defaultRevision=default --create-namespace
helm install istio-ingressgateway istio/gateway -n istio-system
helm install istiod istio/istiod -n istio-system --wait

helm install cinemaabyss .\src\kubernetes\helm --namespace cinemaabyss --create-namespace

kubectl label namespace cinemaabyss istio-injection=enabled --overwrite

kubectl get namespace -L istio-injection

kubectl apply -f .\src\kubernetes\circuit-breaker-config.yaml -n cinemaabyss

```

Тестирование

# fortio
```bash
kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.25/samples/httpbin/sample-client/fortio-deploy.yaml -n cinemaabyss
```

# Get the fortio pod name
```bash
FORTIO_POD=$(kubectl get pod -n cinemaabyss | grep fortio | awk '{print $1}')

kubectl exec -n cinemaabyss $FORTIO_POD -c fortio -- fortio load -c 50 -qps 0 -n 500 -loglevel Warning http://movies-service:8081/api/movies
```
Например,

```bash
kubectl exec -n cinemaabyss fortio-deploy-b6757cbbb-7c9qg  -c fortio -- fortio load -c 50 -qps 0 -n 500 -loglevel Warning http://movies-service:8081/api/movies
```

Вывод будет типа такого

```bash
IP addresses distribution:
10.106.113.46:8081: 421
Code 200 : 79 (15.8 %)
Code 500 : 22 (4.4 %)
Code 503 : 399 (79.8 %)
```
Можно еще проверить статистику

```bash
kubectl exec -n cinemaabyss fortio-deploy-b6757cbbb-7c9qg -c istio-proxy -- pilot-agent request GET stats | grep movies-service | grep pending
```

И там смотрим 

```bash
cluster.outbound|8081||movies-service.cinemaabyss.svc.cluster.local;.upstream_rq_pending_total: 311 - столько раз срабатывал circuit breaker
You can see 21 for the upstream_rq_pending_overflow value which means 21 calls so far have been flagged for circuit breaking.
```

Приложите скриншот работы circuit breaker'а

Удаляем все
```bash
istioctl uninstall --purge
kubectl delete namespace istio-system
kubectl delete all --all -n cinemaabyss
kubectl delete namespace cinemaabyss
```
