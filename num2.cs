using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        int N = 5; //max patients 
        int M = 3; // doctors
        int T = 3; //max time

        Clinic clinic = new Clinic(N, M, T);
        await clinic.StartSimulation();
    }
}

class Clinic
{
    private Semaphore _waitingRoom;
    private Queue<Patient> _patientQueue = new Queue<Patient>();
    private List<Doctor> _doctors = new List<Doctor>();
    private int _N; 
    private int _M;
    private int _T;
    private object _qlock = new object();
    
    public Clinic(int N, int M, int T)
    {
        _N = N;
        _M = M;
        _T = T;
        _waitingRoom = new Semaphore(N, N);

        for (int i = 0; i < M; i++) 
        {
            _doctors.Add(new Doctor());
        }
    }
    public async Task StartSimulation()
    {
 
        var doctorTasks = new List<Task>();
        foreach (var doctor in _doctors)
        {
            doctorTasks.Add(Task.Run(async () => await DoctorConsultation(doctor)));
        }

        while (true)
        {

            Patient newPatient = GeneratePatient();

            await Log($"{(newPatient.IsSick ? "Sick" : "Healthy")} patient {newPatient.Id} come");

            if (_waitingRoom.WaitOne(0))
            {
                lock (_qlock)
                {
                    _patientQueue.Enqueue(newPatient);
                }
               

                await Log($"Patient {newPatient.Id} enters the waiting room");
            }
            else
            {

                await Log($"The waiting room is full. Patient {newPatient.Id} is joining the queue.");
            }

            await Task.Delay(1000);  // waiting 1000ms
        }
    }
    private async Task DoctorConsultation(Doctor doctor)
    {
        while (true)
        {
            Patient patient = null;
            lock (_qlock)
            {
                if (_patientQueue.Count > 0)
                {
                    patient = _patientQueue.Dequeue();
                }
            }

            if (patient != null)
            {
                
                await Log($"Doctor {doctor.Id} begins consultation with {(patient.IsSick ? "sick" : "health")} patient {patient.Id}");

                await doctor.Сonsultation(patient, _T);

                _waitingRoom.Release();
            }
            else
            {
                await Task.Delay(1000);
            }
        }
    }
    private Patient GeneratePatient()
    {
        return new Patient(new Random().Next(0, 2) == 0 );
    }
    public async Task Log(string message)
    {
        Console.WriteLine($"{DateTime.Now}: {message}");

        //запись в файл
        using (StreamWriter writer = new StreamWriter("clinic_log.txt", append: true))
        {
            await writer.WriteLineAsync($"{DateTime.Now}: {message}");
        }

    }

}
class Patient
{
    
    public int Id { get; }
    public bool IsSick { get; set; }
    public DateTime ArrivalTime { get; set; }
    
    private static int idCounter = 0; //Статическая переменная для хранения последнего присвоенного идентификатора

    public Patient(bool isSick)
    {
        Id = Interlocked.Increment(ref idCounter);
        IsSick = isSick;
        ArrivalTime = DateTime.Now;
    }

}
class Doctor
{ 
    public int Id { get; }

    private static int idCounter = 0;
    private static Random rand = new Random();

    public Doctor()
    {
        Id = Interlocked.Increment(ref idCounter);
    }

    public async Task Сonsultation(Patient patient, int T)
    {
        int consultationTime = rand.Next(1, T + 1);
        
        await Log($"Doctor {Id} starts consultation of {(patient.IsSick ? "sick" : "healthy")} patient {patient.Id}, expected consultation time {consultationTime}");
        
        await Task.Delay(consultationTime * 1000);
        
        await Log($"Doctor {Id} end consultation of {(patient.IsSick ? "sick" : "healthy")} patient {patient.Id}");
    }
    public async Task Log(string message)
    {
        Console.WriteLine($"{DateTime.Now}: {message}");

        using (StreamWriter writer = new StreamWriter("clinic_log.txt", append: true))
        {
            await writer.WriteLineAsync($"{DateTime.Now}: {message}");
        }

    }
}
