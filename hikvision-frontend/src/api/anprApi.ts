import axios from "axios";

const API_URL = "http://localhost:5018/api";

export interface AnprEvent {
  id: number;
  cameraId: number;
  cameraName: string;
  plateNumber: string;
  vehicleType: string;
  imageUrl: string;
  detectedAt: string;
}

export const getLatestAnpr = async (): Promise<AnprEvent> => {
  const response = await axios.get<AnprEvent>(
    `${API_URL}/Anpr/latest`
  );

  return response.data;
};