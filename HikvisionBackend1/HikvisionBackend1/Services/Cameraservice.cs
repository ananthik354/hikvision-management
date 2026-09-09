using HikvisionBackend1.Data;
using HikvisionBackend1.DTOs;
using HikvisionBackend1.Interfaces;
using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class Cameraservice : ICamera
    {
        private readonly AppDbContext Context;

        public Cameraservice(AppDbContext _Context)
        {
            Context = _Context;
        }

        // =========================
        // ADD CAMERA
        // =========================

        public async Task<ResponceDTO> AddCamera(Camera Model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Model.Name))
                {
                    return new ResponceDTO(
                        false,
                        "Camera name is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(Model.IpAddress))
                {
                    return new ResponceDTO(
                        false,
                        "Camera IP address is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(Model.Location))
                {
                    return new ResponceDTO(
                        false,
                        "Camera location is required"
                    );
                }

                // =========================
                // CHECK DUPLICATE IP
                // =========================

                var existingCamera =
                    await Context.Cameras
                        .FirstOrDefaultAsync(x =>
                            x.IpAddress == Model.IpAddress);

                if (existingCamera != null)
                {
                    return new ResponceDTO(
                        false,
                        "Camera with this IP already exists"
                    );
                }

                // =========================
                // DEFAULT ISAPI VALUES
                // =========================

                if (Model.HttpPort <= 0)
                    Model.HttpPort = 80;

                if (Model.SdkPort <= 0)
                    Model.SdkPort = 8000;

                if (Model.IsapiChannel <= 0)
                    Model.IsapiChannel = 1;

                // New camera starts offline
                Model.Status = "Offline";

                Model.IsIsapiOnline = false;

                await Context.Cameras.AddAsync(Model);

                await Context.SaveChangesAsync();

                return new ResponceDTO(
                    true,
                    "Camera added successfully"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("ADD CAMERA ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(
                    false,
                    ex.Message
                );
            }
        }


        // =========================
        // GET ALL CAMERAS
        // =========================

        public async Task<ResponceDTO> GetAllCameras()
        {
            try
            {
                var cameras =
                    await Context.Cameras
                        .AsNoTracking()
                        .OrderBy(x => x.Id)
                        .ToListAsync();

                return new ResponceDTO(cameras);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET CAMERAS ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(
                    false,
                    ex.Message
                );
            }
        }


        // =========================
        // GET CAMERA BY ID
        // =========================

        public async Task<ResponceDTO> GetCameraById(int id)
        {
            try
            {
                var camera =
                    await Context.Cameras
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == id);

                if (camera == null)
                {
                    return new ResponceDTO(
                        false,
                        "Camera not found"
                    );
                }

                return new ResponceDTO(camera);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET CAMERA ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(
                    false,
                    ex.Message
                );
            }
        }


        // =========================
        // UPDATE CAMERA
        // =========================

        public async Task<ResponceDTO> UpdateCamera(
            int id,
            Camera Model)
        {
            try
            {
                var camera =
                    await Context.Cameras
                        .FirstOrDefaultAsync(x =>
                            x.Id == id);

                if (camera == null)
                {
                    return new ResponceDTO(
                        false,
                        "Camera not found"
                    );
                }

                // =========================
                // VALIDATION
                // =========================

                if (string.IsNullOrWhiteSpace(Model.Name))
                {
                    return new ResponceDTO(
                        false,
                        "Camera name is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(Model.IpAddress))
                {
                    return new ResponceDTO(
                        false,
                        "Camera IP address is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(Model.Location))
                {
                    return new ResponceDTO(
                        false,
                        "Camera location is required"
                    );
                }

                // =========================
                // CHECK DUPLICATE IP
                // =========================

                var duplicate =
                    await Context.Cameras
                        .FirstOrDefaultAsync(x =>
                            x.IpAddress == Model.IpAddress &&
                            x.Id != id);

                if (duplicate != null)
                {
                    return new ResponceDTO(
                        false,
                        "Another camera already uses this IP"
                    );
                }

                // =========================
                // BASIC CAMERA DETAILS
                // =========================

                camera.Name =
                    Model.Name.Trim();

                camera.IpAddress =
                    Model.IpAddress.Trim();

                camera.Location =
                    Model.Location.Trim();

                // =========================
                // ISAPI DETAILS
                // =========================

                camera.HttpPort =
                    Model.HttpPort > 0
                        ? Model.HttpPort
                        : 80;

                camera.SdkPort =
                    Model.SdkPort > 0
                        ? Model.SdkPort
                        : 8000;

                camera.HikvisionUsername =
                    Model.HikvisionUsername;

                camera.HikvisionPassword =
                    Model.HikvisionPassword;

                camera.IsapiChannel =
                    Model.IsapiChannel > 0
                        ? Model.IsapiChannel
                        : 1;

                // =========================
                // FTP DETAILS
                // KEEP FOR NOW
                // =========================

                camera.FtpHost =
                    Model.FtpHost;

                camera.FtpPort =
                    Model.FtpPort;

                camera.FtpUsername =
                    Model.FtpUsername;

                camera.FtpPassword =
                    Model.FtpPassword;

                camera.FtpFolder =
                    Model.FtpFolder;

                // =========================
                // STATUS
                // =========================

                // Do not overwrite the live ISAPI status
                // from frontend accidentally.
                //
                // Status can still be manually changed later
                // if required.

                if (!string.IsNullOrWhiteSpace(Model.Status))
                {
                    camera.Status = Model.Status;
                }

                await Context.SaveChangesAsync();

                return new ResponceDTO(
                    true,
                    "Camera updated successfully"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("UPDATE CAMERA ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(
                    false,
                    ex.Message
                );
            }
        }


        // =========================
        // DELETE CAMERA
        // =========================

        public async Task<ResponceDTO> DeleteCamera(int id)
        {
            try
            {
                var camera =
                    await Context.Cameras
                        .FirstOrDefaultAsync(x =>
                            x.Id == id);

                if (camera == null)
                {
                    return new ResponceDTO(
                        false,
                        "Camera not found"
                    );
                }

                Context.Cameras.Remove(camera);

                await Context.SaveChangesAsync();

                return new ResponceDTO(
                    true,
                    "Camera deleted successfully"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("DELETE CAMERA ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(
                    false,
                    ex.Message
                );
            }
        }
    }
}