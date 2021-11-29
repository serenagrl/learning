echo "`n***** Creating Secrets ..."
kubectl apply -f .\secret.yaml

echo "`n***** Creating ConfigMaps ..."
kubectl apply -f .\configmap.yaml

echo "`n***** Creating Redis Master and Replicas ..."
kubectl apply -f .\redis-statefulset.yaml

echo "`n***** Waiting for pods ..."
kubectl rollout status statefulsets redis

echo "`n***** Waiting Redis Master and Replicas to initialize ..."
Start-Sleep -s 8

echo "`n***** Creating Redis Sentinels ..."
kubectl apply -f .\sentinel-statefulset.yaml

echo "`n***** Waiting for sentinel pods ..."
kubectl rollout status statefulsets sentinel

echo "`n***** Listing all pods ..."
kubectl get pods -o wide

echo "`n***** Installation script executed!"