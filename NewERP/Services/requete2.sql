-- 1. VALEURS ACTUELLES DES COMPOSANTS PAR EMPLOYÉ (dernière fiche de paie)
SELECT 
    e.name as employee_id,
    e.employee_name,
    ss.name as salary_slip_name,
    ss.start_date,
    ss.end_date,
    sd.salary_component,
    sc.component_name,
    sc.type as component_type, -- 'Earning' ou 'Deduction'
    sd.amount,
    sd.formula,
    ss.gross_pay,
    ss.total_deduction,
    ss.net_pay
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
JOIN `tabSalary Component` sc ON sd.salary_component = sc.name
WHERE ss.docstatus = 1 -- Fiches soumises seulement
AND ss.start_date = (
    SELECT MAX(start_date) 
    FROM `tabSalary Slip` ss2 
    WHERE ss2.employee = e.name 
    AND ss2.docstatus = 1
)
ORDER BY e.employee_name, sc.type, sd.salary_component;

-- 2. COMPOSANTS SPÉCIFIQUES POUR UN EMPLOYÉ
SELECT 
    e.employee_name,
    sd.salary_component,
    sd.amount,
    sd.formula,
    ss.start_date,
    ss.posting_date
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
WHERE e.name = 'EMP-001' -- Remplacer par l'ID employé
AND ss.docstatus = 1
AND sd.salary_component IN ('Salaire Base', 'Impot', 'Indemnité')
ORDER BY ss.start_date DESC;

-- 3. HISTORIQUE DES VALEURS D'UN COMPOSANT POUR TOUS LES EMPLOYÉS
SELECT 
    e.name as employee_id,
    e.employee_name,
    ss.start_date,
    ss.end_date,
    sd.amount,
    sd.formula
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
WHERE sd.salary_component = 'Salaire Base' -- Composant spécifique
AND ss.docstatus = 1
ORDER BY e.employee_name, ss.start_date DESC;

-- 4. STRUCTURE SALARIALE ACTUELLE PAR EMPLOYÉ
SELECT 
    e.name as employee_id,
    e.employee_name,
    ssa.salary_structure,
    ssa.base as salaire_base,
    ssa.from_date,
    ssa.to_date,
    ss_struct.name as structure_name
FROM `tabEmployee` e
JOIN `tabSalary Structure Assignment` ssa ON e.name = ssa.employee
LEFT JOIN `tabSalary Structure` ss_struct ON ssa.salary_structure = ss_struct.name
WHERE ssa.docstatus = 1
AND (ssa.to_date IS NULL OR ssa.to_date >= CURDATE())
ORDER BY e.employee_name;

-- 5. DÉTAIL COMPLET AVEC FORMULES ET CALCULS
SELECT 
    e.name as employee_id,
    e.employee_name,
    ss.name as slip_name,
    ss.start_date,
    sd.salary_component,
    sc.component_name,
    sc.type as component_type,
    sd.amount,
    sd.formula,
    sd.condition,
    sd.statistical_component,
    CASE 
        WHEN sc.type = 'Earning' THEN sd.amount 
        ELSE 0 
    END as earning_amount,
    CASE 
        WHEN sc.type = 'Deduction' THEN sd.amount 
        ELSE 0 
    END as deduction_amount
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
JOIN `tabSalary Component` sc ON sd.salary_component = sc.name
WHERE ss.docstatus = 1
AND ss.start_date >= DATE_SUB(CURDATE(), INTERVAL 6 MONTH) -- 6 derniers mois
ORDER BY e.employee_name, ss.start_date DESC, sc.type;

-- 6. RÉSUMÉ PAR EMPLOYÉ ET PAR COMPOSANT (MOYENNE/MAX/MIN)
SELECT 
    e.name as employee_id,
    e.employee_name,
    sd.salary_component,
    COUNT(*) as nb_fiches,
    AVG(sd.amount) as montant_moyen,
    MIN(sd.amount) as montant_min,
    MAX(sd.amount) as montant_max,
    MAX(ss.start_date) as derniere_fiche
FROM `tabEmployee` e
JOIN `tabSalary Slip` ss ON e.name = ss.employee
JOIN `tabSalary Detail` sd ON ss.name = sd.parent
WHERE ss.docstatus = 1
AND ss.start_date >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
GROUP BY e.name, sd.salary_component
ORDER BY e.employee_name, sd.salary_component;

-- 7. EMPLOYÉS AVEC COMPOSANT SPÉCIFIQUE AU-DESSUS/EN-DESSOUS D'UN SEUIL
SELECT 
    e.name as employee_id,
    e.employee_name,
    sd.salary_component,
    sd.amount,
    ss.start_date,
    CASE 
        WHEN sd.amount > 50000 THEN 'Au-dessus du seuil'
        ELSE 'En-dessous du seuil'
    END as statut_seuil
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
AND sd.amount > 30000 -- Seuil personnalisable
ORDER BY sd.amount DESC;

-- 8. COMPOSANTS AVEC FORMULES ACTIVES
SELECT DISTINCT
    sc.name as component_name,
    sc.type,
    sc.formula_based_on_taxable_salary,
    sc.condition,
    sc.formula,
    sc.amount_based_on_formula,
    COUNT(sd.name) as utilisation_count
FROM `tabSalary Component` sc
LEFT JOIN `tabSalary Detail` sd ON sc.name = sd.salary_component
WHERE sc.disabled = 0
GROUP BY sc.name
ORDER BY utilisation_count DESC;