echo "`n***** Removing Redis Sentinels ..."
kubectl delete -f .\sentinel-statefulset.yaml

echo "`n***** Removing Redis Master and Replicas ..."
kubectl delete -f .\redis-statefulset.yaml

echo "`n***** Removing ConfigMaps ..."
kubectl delete -f .\configmap.yaml

echo "`n***** Removing Secrets ..."
kubectl delete -f .\secret.yaml

echo "`n***** Removing Persistent Volume Claims ..."
kubectl delete pvc data-redis-0 data-redis-1 data-redis-2 data-sentinel-0 data-sentinel-1 data-sentinel-2 

echo "`n***** Uninstallation script executed!"