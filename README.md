## RabbitMQ commands
| Description | Command |
|------------------|-----------------|
| Fireup the RabbitMQ server     | `rabbitmq-sever`    | 
| Fireup the RabbitMQ server detached from terminal    | `rabbitmq-sever -detached`    | 
| Stop the RabbitMQ server     | `rabbitmqctl stop -n [erlang node name]@[host name]`    |
| Stop the RabbitMQ application     | `rabbitmqctl stop_app -n [erlang node name]@[host name]`    |
| Check the status of the server     | `rabbitmqctl status`     |
| Add vhost     | `rabbitmqctl add_vhost [vhost name] -n [erlang node name]@[host name]`     |
| Delete vhost     | `rabbitmqctl delete_vhost [vhost name] -n [erlang node name]@[host name]`     |
| List vhost     | `rabbitmqctl list_vhosts -n [erlang node name]@[host name]`     |
| Add user     | `rabbitmqctl add_user [username] [password]`     |
| Delete user     | `rabbitmq delete_user [username]`     |
| List user     | `rabbitmq list_user`    |
| Change user password     | `rabbitmqctl change_password [username] [password]`    |
| Setting user permission. <br>"" : To match no *queue* or exchagne name <br> "a-.": Any queue or exchange that starts with "a-" <br> ".*": Any queue or exchange <br><br> First pattern: *Config* permission <br> Second pattern: *Write* permmission <br> Third pattern: *Read* permission | `rabbitmqctl set_permissions -p [vhost name] [username] "" "a.*" ".*"`     |
| Clear all permissions for the user on the specified vhost     | `rabbitmqctl -n [erlang node name]@[host name] clear_permissions -p [vhost name] [username]`    |
| List user permissions    | `rabbitmqctl -n [erlang node name]@[host name] list_user_permissions [username]`    |
| List queues     | `rabbitmqctl -n [erlang node name]@[host name] -p [vhost name] list_queues`    |
| List exchanges     | `rabbitmqctl -n [erlang node name]@[host name] -p [vhost name] list_exchanges`    |
| List bindings     | `rabbitmqctl -n [erlang node name]@[host name] -p [vhost name] list_bindigns`    |
| Set RabbitMQ node name     | `set rabbitmq_nodename=[node name]`    |
| Set RabbitMQ node port     | `set rabbitmq_node_port=[node port]`    |
| Change RabbitMQ config file path     | `set config_file=[new path]`    |
| Order of commands for joining new node to cluster     | 1: `rabbitmqctl -n [new node address] stop_app`<br>2: `rabbitmqctl -n [new node address] reset`<br>3: `rabbitmqctl -n [new node address] join_cluster --[ram/disk] [disk node address 1] [disk node address 2] ... [disk node address n]`<br> 4: `rabbitmqctl -n [new node address] start_app`    |
| *reset* empties the node of its metadata and restores it to an empty state (When the node being reset is a part of a cluster, the command also communicates with the disk nodes in the cluster to tell them that the node is leaving)  | `rabbitmqctl [node address] reset`|

## Erlang commands
| Description | Command |
|------------------|-----------------|
|  Create new erlang node with given short name    | `erl -sname [node name]`    |
|  Show the erlang node name    | `node().`    |
|  Show other erlang nodes run on the machine    | `net_adm:names().`    |
|  Ping other node    | `net_adm:ping('[erlang node name]@[hostname]')`    |
|  Close the erlang node    | `rpc:call([earlang node name]@[hostname], [module], [function], [[arguments]])`    |  
|  Close the erlang node    | `net_adm:q().`    |  

## Importain RabbitMQ files
| Description | Path |
|------------------|-----------------|
|  RabbitMQ log file    | `/var/log/rabbitmq/[erlang node name]@[hostname].log`    |
|  RabbitMQ config file (systemwide tunables and settings)    | `/etc/rabbitmq/rabbitmq.config`    |
|  Erlagn cookie    | *usually located in the user’s home directory*    |
