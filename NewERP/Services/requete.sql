--Employee--
SELECT name, employee_name, status, department FROM `tabEmployee`;

-----Alea 2-------
-- Pour trouver les employés avec un composant au-dessus d'un seuil
SELECT 
    e.name as employee_id,
    e.employee_name,
    ss.name as slip_name,
    sd.salary_component,
    sd.amount
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
WHERE sd.salary_component = 'Salaire Base' 
AND ss.docstatus = 1
AND ss.start_date = (
    SELECT MAX(start_date) 
    FROM `tabSalary Slip` ss2 
    WHERE ss2.employee = e.name 
    AND ss2.docstatus = 1
)
AND sd.amount < 850000; 
