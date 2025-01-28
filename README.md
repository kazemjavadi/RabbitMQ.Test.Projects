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
| Setting user permission     | `rabbitmqctl set_permissions [vhost name] [username] "" "a.*" ".*"`     |
| Clear permissions     | `rabbitmqctl clear_permissions -p [vhost name] [username]`    |
| List user permissions    | `rabbitmqctl list_user_permissions [username]`    |
| List queues     | `rabbitmqctl list_queues -p [vhost name]`    |
| List exchanges     | `rabbitmqctl list_exchanges`    |
| List bindings     | `rabbitmqctl list_bindigns`    |
| Set RabbitMQ node name     | `set rabbitmq_nodename=[node name]`    |
| Set RabbitMQ node port     | `set rabbitmq_node_port=[node port]`    |
| Change RabbitMQ config file path     | `set config_file=[new path]`    |

## Erlang commands
| Description | Command |
|------------------|-----------------|
|  Create new erlang node with given short name    | `erl -sname [node name]`    |
|  Show the erlang node name    | `node().`    |
|  Show other erlang nodes run on the machine    | `net_adm:names().`    |
|  Ping other node    | `net_adm:ping('[erlang node]@[hostname]')`    |
|  Close the erlang node    | `net_adm:q().`    |

## Importain RabbitMQ files
| Description | Path |
|------------------|-----------------|
|  RabbitMQ log file    | `/var/log/rabbitmq/[erlang node name]@[hostname].log`    |
|  RabbitMQ config file (systemwide tunables and settings)    | `/etc/rabbitmq/rabbitmq.config`    |
