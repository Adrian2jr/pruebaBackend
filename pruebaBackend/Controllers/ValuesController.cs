using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;


namespace pruebaBackend.Controllers
{
    public class Conexion
    {
        public static MySqlConnection ObtenerConexion()
        {
            MySqlConnection conectar = new MySqlConnection("server=localhost; database=todoapp; Uid=root; pwd=guerrero2;");
            conectar.Open();
            return conectar;
        }
    }

    public class ValuesController : ApiController
    {
        [HttpGet]
        [Route("api/tareas/{userId}")]
        public IHttpActionResult GetTasksByUserId(int userId)
        {
            try
            {
                MySqlConnection conectar = Conexion.ObtenerConexion();
                MySqlCommand comando = new MySqlCommand("SELECT * FROM tareas WHERE userId = @userId", conectar);
                comando.Parameters.AddWithValue("@userId", userId);

                MySqlDataReader lector = comando.ExecuteReader();
                List<Task> lista = new List<Task>();

                while (lector.Read())
                {
                    Task task = new Task
                    {
                        id = lector.GetInt32(0),
                        title = lector.GetString(1),
                        description = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                        completed = lector.GetBoolean(3),
                        createdAt = lector.GetDateTime(4),
                        userId = lector.GetInt32(5)
                    };
                    lista.Add(task);
                }

                conectar.Close();

                return Json(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/tareas")]
        public IHttpActionResult PostNewTask([FromBody] Task newTask)
        {
            try
            {
                MySqlConnection conectar = Conexion.ObtenerConexion();
                MySqlCommand comando = new MySqlCommand("INSERT INTO tareas (title, description, completed, createdAt, userId) VALUES (@title, @description, @completed, @createdAt, @userId); SELECT LAST_INSERT_ID();", conectar);
                comando.Parameters.AddWithValue("@title", newTask.title);
                comando.Parameters.AddWithValue("@description", newTask.description);
                comando.Parameters.AddWithValue("@completed", newTask.completed);
                comando.Parameters.AddWithValue("@createdAt", newTask.createdAt);
                comando.Parameters.AddWithValue("@userId", newTask.userId);

                int filasAfectadas = comando.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    // Obtiene el ID del objeto recién insertado
                    long taskId = comando.LastInsertedId;

                    // Realiza una consulta para obtener el objeto recién creado
                    MySqlCommand obtenerComando = new MySqlCommand("SELECT * FROM tareas WHERE id = @taskId", conectar);
                    obtenerComando.Parameters.AddWithValue("@taskId", taskId);
                    MySqlDataReader reader = obtenerComando.ExecuteReader();

                    if (reader.Read())
                    {
                        Task tareaCreada = new Task
                        {
                            id = reader.GetInt32("id"),
                            title = reader.GetString("title"),
                            description = reader.GetString("description"),
                            completed = reader.GetBoolean("completed"),
                            createdAt = reader.GetDateTime("createdAt"),
                            userId = reader.GetInt32("userId")
                        };

                        reader.Close();

                        return Ok(new
                        {
                            Message = "Tarea creada exitosamente",
                            Task = tareaCreada
                        });
                    }
                    else
                    {
                        reader.Close();
                        return BadRequest("No se pudo obtener la tarea recién creada");
                    }
                }
                else
                {
                    return BadRequest("No se pudo crear la tarea");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/tareas")]
        public IHttpActionResult PutTask([FromBody] Task updatedTask)
        {
            try
            {
                MySqlConnection conectar = Conexion.ObtenerConexion();
                MySqlCommand comando = new MySqlCommand("UPDATE tareas SET title = @title, description = @description, completed = @completed WHERE id = @id", conectar);
                comando.Parameters.AddWithValue("@title", updatedTask.title);
                comando.Parameters.AddWithValue("@description", updatedTask.description);
                comando.Parameters.AddWithValue("@completed", updatedTask.completed);
                comando.Parameters.AddWithValue("@id", updatedTask.id);

                int filasAfectadas = comando.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    // Realiza una consulta para obtener el objeto actualizado
                    MySqlCommand obtenerComando = new MySqlCommand("SELECT * FROM tareas WHERE id = @id", conectar);
                    obtenerComando.Parameters.AddWithValue("@id", updatedTask.id);
                    MySqlDataReader reader = obtenerComando.ExecuteReader();

                    if (reader.Read())
                    {
                        Task tareaActualizada = new Task
                        {
                            id = reader.GetInt32("id"),
                            title = reader.GetString("title"),
                            description = reader.GetString("description"),
                            completed = reader.GetBoolean("completed"),
                            createdAt = reader.GetDateTime("createdAt"),
                            userId = reader.GetInt32("userId")
                        };

                        reader.Close();

                        return Ok(new
                        {
                            Message = "Tarea actualizada exitosamente",
                            Task = tareaActualizada
                        });
                    }
                    else
                    {
                        reader.Close();
                        return BadRequest("No se pudo obtener la tarea actualizada");
                    }
                }
                else
                {
                    return BadRequest("No se pudo actualizar la tarea");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("api/tareas/{id}")]
        public IHttpActionResult DeleteTask(int id)
        {
            try
            {
                MySqlConnection conectar = Conexion.ObtenerConexion();
                MySqlCommand comando = new MySqlCommand("DELETE FROM tareas WHERE id = @id", conectar);
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    return Ok("Tarea eliminada exitosamente");
                }
                else
                {
                    return BadRequest("No se pudo eliminar la tarea");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/usuarios/login")]
        public IHttpActionResult Login([FromBody] User user)
        {
            try
            {
                MySqlConnection conectar = Conexion.ObtenerConexion();
                MySqlCommand comando = new MySqlCommand("SELECT id, username FROM usuarios WHERE email = @email AND password = @password", conectar);
                comando.Parameters.AddWithValue("@email", user.email);
                comando.Parameters.AddWithValue("@password", user.password);

                MySqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    // Autenticación exitosa, obtener el id y username
                    int userId = lector.GetInt32(0);
                    string username = lector.GetString(1);

                    var resultado = new { ok = true, msg = "Login Exitoso", id = userId, username = username };
                    return Ok(resultado);
                }
                else
                {
                    // Autenticación fallida
                    var resultado = new { ok = false, msg = "Usuario/contraseña incorrectos" };
                    return Ok(resultado);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }

    public class Task
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool completed { get; set; }
        public DateTime createdAt { get; set; }
        public int userId { get; set; }
    }
}